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
    const [addIp, setAddIp] = useState('');
    const [addStream, setAddStream] = useState('');

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
            const response = await fetch(`https://localhost:7193/Robots/user/${currentUserId}`, {
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
        if (!addName || !addIp) {
            alert("Заповніть основні поля!");
            return;
        }

        const newData = {
            roboName: addName,
            roboIpAdress: addIp,
            status: "new",
            streamUrl: addStream,
            userId: parseInt(currentUserId)
        };

        try {
            const response = await fetch('https://localhost:7193/Robots', {
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
                setAddIp('');
                setAddStream('');
                loadRobots();
            } else {
                alert("Помилка при збереженні.");
            }
        } catch {
            alert("Помилка мережі при додаванні.");
        }
    }

    function startControl(roboId) {
        let url = `/control?id=${roboId}`;
        if (ownerIdFromUrl) url += `&ownerId=${ownerIdFromUrl}`;
        navigate(url);
    }

    const filteredRobots = allRobots.filter(r =>
        r.roboName.toLowerCase().includes(searchQuery.toLowerCase())
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
                    <p>Завантаження роботів...</p>
                ) : errorMsg ? (
                    <p>{errorMsg}</p>
                ) : filteredRobots.length === 0 ? (
                    <p>Роботів не знайдено.</p>
                ) : (
                    filteredRobots.map(robot => (
                        <div className="robot-card" key={robot.roboId}>
                            <h3>🤖 {robot.roboName}</h3>
                            <p>Статус: <b>{robot.status || 'new'}</b></p>
                            <p>IP: {robot.roboIpAdress || '---'}</p>
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
                        <input type="text" placeholder="Наприклад: Naruto" value={addName} onChange={(e) => setAddName(e.target.value)} />

                        <label>IP Адреса:</label>
                        <input type="text" placeholder="192.168.0.100" value={addIp} onChange={(e) => setAddIp(e.target.value)} />

                        <label>URL Стріму:</label>
                        <input type="text" placeholder="http://..." value={addStream} onChange={(e) => setAddStream(e.target.value)} />

                        <button className="primary-btn" onClick={handleAddRobot}>Створити</button>
                        <button className="btn-secondary" onClick={() => setIsModalOpen(false)}>
                            Скасувати
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Robots;