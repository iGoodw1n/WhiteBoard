// src/components/MainPage.js
import { Tldraw } from '@tldraw/tldraw';
import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

const MainPage = () => {
  const [data, setData] = useState([]);
  const [inputValue, setInputValue] = useState('');

  const handleInputChange = (event) => {
    setInputValue(event.target.value);
  };

  useEffect(() => {
    // Function to fetch data from the server
    const fetchData = async () => {
      try {
        const response = await fetch(window.location.origin + '/boards'); // Replace with your API endpoint
        if (!response.ok) {
          throw new Error('Failed to fetch data');
        }

        const result = await response.json();
        setData(result); // Set the fetched data to the state
      } catch (error) {
        console.error('Error fetching data:', error);
      }
    };

    fetchData(); // Invoke the fetch function

  }, []); // The empty dependency array ensures that the effect runs only once, equivalent to componentDidMount

  return (
    <>
      <div style={{
        textAlign: "center",
        marginTop: "30px",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        gap: "10px"
      }}>
        <h1>Choose whiteboard or</h1>
        <input
        type="text"
        value={inputValue}
        onChange={handleInputChange}
      />
        <Link
          className='btn btn-success btn-md fs-5'
          to={{
            pathname: '/board',
            search: `?boardId=${inputValue}`,
          }}
        >
          Create board
        </Link>
      </div>

      <div style={{
        display: "flex",
        gap: "10px",
        rowGap: "40px",
        marginTop: "20px",
        flexWrap: "wrap",
        justifyContent: "center"
        }}>
        {Object.entries(data).map(([boardId, snapshot]) => (

          <div key={boardId} style={{ width: "300px", height: "300px", textAlign: "center" }}>
            <Link
              to={{
                pathname: '/board',
                search: `?boardId=${boardId}`,
              }}
            >
              <Tldraw
                className='unclickable'
                snapshot={snapshot}
                hideUi
                forceMobile
                onMount={(editor) => {
                  editor.updateInstanceState({ isReadonly: true })
                  editor.zoomToContent()
                  editor.removeAllListeners('change')
                }}
              />
            </Link>
            <text className='fs-5'>{boardId}</text>
          </div>
        ))}
      </div>
    </>
  );
};

export default MainPage;