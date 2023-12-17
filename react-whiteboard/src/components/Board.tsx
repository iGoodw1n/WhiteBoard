import {
  Tldraw,
  track,
  useEditor,
  TLUiAssetUrlOverrides,
  TLUiOverrides,
  toolbarItem
} from '@tldraw/tldraw'
import React from 'react';
import '@tldraw/tldraw/tldraw.css'
import '../styles.css'
import { useLocation } from 'react-router-dom';
import { useSignalR } from '../useSignalR.ts'
import { ScreenshotTool } from '../ScreenshotTool/ScreenshotTool.ts'

const customTools = [ScreenshotTool]

const customUiOverrides: TLUiOverrides = {
	tools: (editor, tools) => {
		return {
			...tools,
			screenshot: {
				id: 'screenshot',
				label: 'Screenshot',
				readonlyOk: false,
				icon: 'tool-screenshot',
				kbd: 'j',
				onSelect() {
					editor.setCurrentTool('screenshot')
				},
			},
		}
	},
	toolbar: (_editor, toolbarItems, { tools }) => {
		toolbarItems.splice(4, 0, toolbarItem(tools.screenshot))
		return toolbarItems
	},
}

const customAssetUrls: TLUiAssetUrlOverrides = {
	icons: {
		'tool-screenshot': '/tool-screenshot.svg',
	},
}

export default function SyncExample() {
  const location = useLocation();
  const boardId = new URLSearchParams(location.search).get('boardId')!;

	const store = useSignalR({
    hostUrl: window.location.origin + '/board',
		boardId: boardId,
	})


  return (
    <div style={{ position: "fixed", inset: 0 }}>
      <Tldraw
        store={store}
        shareZone={<NameEditor />}
				tools={customTools}
				overrides={customUiOverrides}
				assetUrls={customAssetUrls}
			/>
    </div>
  );
};


const NameEditor = track(() => {
  const editor = useEditor()

  const { color, name } = editor.user

  return (
    <div style={{ pointerEvents: 'all', display: 'flex' }}>
      <input
        type="color"
        value={color}
        onChange={(e) => {
          editor.user.updateUserPreferences({
            color: e.currentTarget.value,
          })
        }}
      />
      <input
        value={name}
        onChange={(e) => {
          editor.user.updateUserPreferences({
            name: e.currentTarget.value,
          })
        }}
      />
    </div>
  )
})