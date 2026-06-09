import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import './App.css';

function Robots() {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();

    const token = localStorage.getItem('token');
    const ownerIdFromUrl = searchParams.get('ownerId');

    const [allRobots, setAllRobots] = useState([]);
    const [searchQuery, setSearchQuery] = useState('');
    const [isLoading, setIsLoading] = useState(true);
    const [errorMsg, setErrorMsg] = useState('');

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [addName, setAddName] = useState('');

    function getUserIdFromToken(tokenStr) {
        if (!tokenStr) return null;
        try {
            const base64Url = tokenStr.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const payload = JSON.parse(window.atob(base64));

            return payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
                payload.nameid ||
                payload.sub ||
                payload.id;
        } catch (e) {
            console.error("Помилка парсингу токена:", e);
            return null;
        }
    }

    const currentUserId = ownerIdFromUrl || getUserIdFromToken(token);

    useEffect(() => {
        if (!token) {
            navigate('/login');
            return;
        }

        if (!currentUserId) {
            setTimeout(() => {
                setErrorMsg("Помилка: не вдалося визначити користувача. Перезайдіть у систему.");
                setIsLoading(false);
            }, 0);
            return;
        }

        loadRobots();
    }, [token, currentUserId]);

    async function loadRobots() {
        setIsLoading(true);
        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/Robots/user/${currentUserId}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (response.ok) {
                const data = await response.json();
                setAllRobots(data);
                setErrorMsg('');
            } else {
                setAllRobots([]);
            }
        } catch (error) {
            console.error("Серверна помилка:", error);
            setErrorMsg('Сервер не відповідає');
        } finally {
            setIsLoading(false);
        }
    }

    async function handleAddRobot() {
        if (!addName) {
            alert("Будь ласка, введіть назву робота!");
            return;
        }

        const newData = {
            roboName: addName,
            status: "pending_activation",
            userId: currentUserId,
            secretToken: ""
        };

        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/Robots`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(newData)
            });

            if (response.ok) {
                alert("Робота успішно додано!");
                setIsModalOpen(false);
                setAddName('');
                loadRobots();
            } else {
                const errorText = await response.text();
                alert(`Сервер відхилив запит: ${errorText || response.statusText}`);
            }
        } catch (error) {
            console.error("Помилка мережі:", error);
            alert("Не вдалося зв'язатися з сервером.");
        }
    }

    function startControl(roboId) {
        let url = `/control?id=${roboId}`;
        if (ownerIdFromUrl) url += `&ownerId=${ownerIdFromUrl}`;
        navigate(url);
    }

    const filteredRobots = allRobots.filter(r =>
        r.roboName && r.roboName.toLowerCase().includes(searchQuery.toLowerCase())
    );

    return (
        <div className="container">
            <div id="search-section">
                <input
                    type="text"
                    placeholder="Пошук за іменем робота..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                />
                <button className="view-btn">🔍</button>
            </div>

            <div>
                <h1>{ownerIdFromUrl ? "Роботи користувача" : "Всі роботи"}</h1>
                <button className="primary-btn" onClick={() => setIsModalOpen(true)}>
                    + Додати робота
                </button>
            </div>

            <div className="robot-grid">
                {isLoading ? (
                    <p className="welcome-guest-text">Завантаження роботів...</p>
                ) : errorMsg ? (
                    <p style={{ color: 'red' }}>{errorMsg}</p>
                ) : filteredRobots.length === 0 ? (
                    <p className="welcome-guest-text">Роботів не знайдено.</p>
                ) : (
                    filteredRobots.map(robot => (
                        <div className="robot-card" key={robot.roboId}>
                            <h3>🤖 {robot.roboName}</h3>
                            <p>Статус: <span className="welcome-user-text">{robot.status || 'new'}</span></p>
                            <button className="btn-go" onClick={() => startControl(robot.roboId)}>
                                Керувати
                            </button>
                        </div>
                    ))
                )}
            </div>

            {isModalOpen && (
                <div className="modal">
                    <div className="modal-content">
                        <h3>Додати нового робота</h3>

                        <label>Назва:</label>
                        <input
                            type="text"
                            placeholder="Наприклад: Naruto"
                            value={addName}
                            onChange={(e) => setAddName(e.target.value)}
                        />

                        <div style={{ marginTop: '15px', display: 'flex', gap: '10px' }}>
                            <button className="primary-btn" onClick={handleAddRobot}>Створити</button>
                            <button className="btn-secondary" onClick={() => setIsModalOpen(false)}>
                                Скасувати
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Robots;