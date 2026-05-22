import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './App.css';

function Guard() {
    const navigate = useNavigate();
    const token = localStorage.getItem('token');
    const apiBase = 'https://localhost:7193/Alarms';

    const [alarms, setAlarms] = useState([]);
    const [viewMode, setViewMode] = useState('active');
    const [isLoading, setIsLoading] = useState(true);

    const [dateFrom, setDateFrom] = useState('');
    const [dateTo, setDateTo] = useState('');

    useEffect(() => {
        if (!token) {
            navigate('/login');
        } else {
            if (viewMode !== 'search') {
                fetchAlarms();
            }
        }
    }, [token, viewMode]);

    async function fetchAlarms() {
        setIsLoading(true);
        const url = viewMode === 'active' ? `${apiBase}/active` : apiBase;
        try {
            const response = await fetch(url, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (response.ok) {
                const data = await response.json();
                setAlarms(data);
            }
        } catch (error) {
            console.error("Помилка мережі:", error);
        } finally {
            setIsLoading(false);
        }
    }

    async function handleResolveAlarm(id) {
        try {
            const response = await fetch(`${apiBase}/resolve/${id}`, {
                method: 'PUT',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (response.ok) {
                alert("Тривогу успішно закрито");
                if (viewMode === 'search') {
                    handleSearchByDate();
                } else {
                    fetchAlarms();
                }
            }
        } catch (error) {
            console.error("Помилка:", error);
        }
    }

    async function handleSearchByDate() {
        if (!dateFrom || !dateTo) {
            return alert("Оберіть обидві дати!");
        }

        setIsLoading(true);
        try {
            const response = await fetch(`${apiBase}/search?from=${dateFrom}&to=${dateTo}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (response.ok) {
                const data = await response.json();
                setAlarms(data);
                setViewMode('search');
            }
        } catch (error) {
            console.error("Помилка пошуку:", error);
        } finally {
            setIsLoading(false);
        }
    }

    function formatDate(dateString) {
        if (!dateString) return '---';
        const date = new Date(dateString);
        return date.toLocaleString('uk-UA');
    }

    return (
        <div className="container">
            <h1 id="pageTitle">🚨 Пульт моніторингу безпеки</h1>

            <div id="search-section">
                <button
                    className={`filter-toggle-btn ${viewMode === 'active' ? 'active' : ''}`}
                    onClick={() => setViewMode('active')}
                >
                    🔥 Активні
                </button>
                <button
                    className={`filter-toggle-btn ${viewMode === 'all' ? 'active' : ''}`}
                    onClick={() => setViewMode('all')}
                >
                    📜 Історія
                </button>

                <input type="datetime-local" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} />
                <input type="datetime-local" value={dateTo} onChange={(e) => setDateTo(e.target.value)} />

                <button className="view-btn" onClick={handleSearchByDate}>
                    Фільтрувати
                </button>
            </div>

            {isLoading ? (
                <p className="welcome-guest-text" style={{ textAlign: 'center' }}>Оновлення даних пульта охорони...</p>
            ) : alarms.length === 0 ? (
                <p className="welcome-guest-text" style={{ textAlign: 'center' }}>✅ За вказаним запитом тривог не знайдено.</p>
            ) : (
                <table>
                    <thead>
                        <tr>
                            <th>ID інциденту</th>
                            <th>Робот / Джерело</th>
                            <th>Власник (Контакти)</th>
                            <th>Опис загрози</th>
                            <th>Час виникнення</th>
                            <th>Дія оператора</th>
                        </tr>
                    </thead>
                    <tbody>
                        {alarms.map((alarm) => {
                            const isUnresolved = !alarm.isResolved;
                            return (
                                <tr key={alarm.alarmId || alarm.id}>
                                    <td><b>#{alarm.alarmId || alarm.id}</b></td>
                                    <td>🤖 {alarm.roboName || `Робот (ID: ${alarm.roboId})`}</td>

                                    <td>
                                        <div className="welcome-user-text">
                                            {alarm.userEmail || alarm.ownerEmail || "Не вказано"}
                                        </div>
                                        {alarm.userPhone || alarm.ownerPhone ? (
                                            <div>
                                                <a href={`tel:${alarm.userPhone || alarm.ownerPhone}`}>
                                                    📞 {alarm.userPhone || alarm.ownerPhone}
                                                </a>
                                            </div>
                                        ) : (
                                            <div className="welcome-guest-text">Телефон відсутній</div>
                                        )}
                                    </td>

                                    <td>{alarm.message || "Порушення периметру"}</td>
                                    <td>{formatDate(alarm.timestamp)}</td>

                                    <td>
                                        {isUnresolved ? (
                                            <button className="edit-btn" onClick={() => handleResolveAlarm(alarm.alarmId || alarm.id)}>
                                                Закрити тривогу
                                            </button>
                                        ) : (
                                            <span className="welcome-guest-text">Врегульовано</span>
                                        )}
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
            )}
        </div>
    );
}

export default Guard;