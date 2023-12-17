// src/components/CardDetail.js
import React from 'react';
import { useParams } from 'react-router-dom';

const CardDetail = ({ cards }) => {
  const { cardId } = useParams();
  const card = cards.find((c) => c.id === parseInt(cardId, 10));

  if (!card) {
    return <div>Card not found</div>;
  }

  return (
    <div>
      <h1>{card.title}</h1>
      <p>{card.description}</p>
    </div>
  );
};

export default CardDetail;
