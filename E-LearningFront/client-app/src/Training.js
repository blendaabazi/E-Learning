import React, { useState, useEffect } from 'react';

function TrainingList() {
    const [trainings, setTrainings] = useState([]);
    const [error, setError] = useState(null);

    useEffect(() => {
        // Përdorimi i fetch për të marrë të dhënat nga API
        fetch('https://localhost:7246/api/training')  // Adresa e API-së për të marrë trajnime
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();  // Kthejeni përgjigjen në JSON
            })
            .then(data => {
                console.log('Trainings Data:', data);  // Kontrolloni të dhënat në konsolë
                setTrainings(data);  // Vendosni të dhënat në gjendjen e komponentit
            })
            .catch(error => {
                console.error('Error fetching trainings:', error);  // Menaxhoni gabimet
                setError(error.message);  // Vendosni mesazhin e gabimit në gjendjen e komponentit
            });
    }, []);

    return (
        <div>
            <h2>Training List</h2>
            {error && <p>Error: {error}</p>}  {/* Shfaq mesazhin e gabimit nëse ka */}
            <ul>
                {trainings.length > 0 ? (
                    trainings.map(training => (
                        <li key={training.id}>
                            <strong>{training.name}</strong>: {training.description}
                        </li>
                    ))
                ) : (
                    <li>No trainings available</li>
                )}
            </ul>
        </div>
    );
}

export default TrainingList;
