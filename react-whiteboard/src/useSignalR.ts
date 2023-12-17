import {
    HistoryEntry,
    StoreListener,
    TLRecord,
    TLStoreWithStatus,
    createTLStore,
    defaultShapeUtils,
    throttle,
  } from '@tldraw/tldraw';
  import { useEffect, useState } from 'react';
  import * as signalR from '@microsoft/signalr'; // Make sure to import the SignalR library
  
  export function useSignalR({
    hostUrl,
    boardId,
  }: {
    hostUrl: string;
    boardId: string;
  }) {
    const [store] = useState(() => {
      const store = createTLStore({
        shapeUtils: [...defaultShapeUtils],
      });
      // store.loadSnapshot(DEFAULT_STORE)
      return store;
    });
  
    const [storeWithStatus, setStoreWithStatus] = useState<TLStoreWithStatus>({
      status: 'loading',
    });
  
    useEffect(() => {
      const hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${hostUrl}?boardId=${boardId}`)
        .withAutomaticReconnect()
        .build();
        
      setStoreWithStatus({ status: 'loading' });

      hubConnection.on('init', (snapshot) => {
        store.loadSnapshot(snapshot)
        
      })
      hubConnection.on('recovery', (data) => store.loadSnapshot(data.snapshot))
      hubConnection.on('update', (data) => {
        try {
          for (const update of data.updates) {
            store.mergeRemoteChanges(() => {
              const {
                changes: { added, updated, removed },
              } = update as HistoryEntry<TLRecord>

              for (const record of Object.values(added)) {
                store.put([record])
              }
              for (const [, to] of Object.values(updated)) {
                store.put([to])
              }
              for (const record of Object.values(removed)) {
                store.remove([record.id])
              }
            })
          }
        } catch (e) {
          console.error(e)
          hubConnection.invoke('Recovery')
        }
      })
  
      const pendingChanges: HistoryEntry<TLRecord>[] = [];
  
      const sendChanges = throttle(() => {
        if (pendingChanges.length === 0) return;
        hubConnection.invoke('Update', { updates: pendingChanges })
        .catch(e => console.log(e));
        pendingChanges.length = 0;
      }, 32);
  
      const handleChange: StoreListener<TLRecord> = (event) => {
        if (event.source !== 'user') return;
        pendingChanges.push(event);
        sendChanges();
      };
  
      store.listen(handleChange, {
        source: 'user',
        scope: 'document',
      });
      
      hubConnection.start()
        .then(() => {
          setStoreWithStatus({
            status: 'synced-remote',
            connectionStatus: 'online',
            store,
          });
          console.log("Hoorah! Connection established!");
        })
        .catch(error => {
          console.error("Error establishing connection:", error);
        });

  
      return () => {
        hubConnection.stop().then(() => {
          console.log("That is all. Connection has been ended.")
        });
      };
    }, [store, boardId, hostUrl]);
  
    return storeWithStatus;
  }
  