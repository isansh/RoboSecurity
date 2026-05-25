import { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { QRCodeSVG } from 'qrcode.react';
import './App.css';

const RobotControl = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    const currentRobotId = searchParams.get('id');
    const ownerId = searchParams.get('ownerId');
    const token = localStorage.getItem('token');
    const apiBase = 'https://localhost:7193/Robots';

    const [robot, setRobot] = useState(null);
    const [loading, setLoading] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [showQR, setShowQR] = useState(false);

    const [editName, setEditName] = useState('');
    const [editStatus, setEditStatus] = useState('');

    const refreshData = async (id) => {
        try {
            const response = await fetch(`${apiBase}/id/${id}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (response.ok) {
                const data = await response.json();
                setRobot(data);
                setEditName(data.roboName);
                setEditStatus(data.status);
            }
        } catch (error) {
            console.error("Помилка оновлення даних:", error);
        }
    };

    useEffect(() => {
        if (!currentRobotId) {
            alert("ID робота не вказано!");
            setTimeout(() => setLoading(false), 0);
            return;
        }

        let isMounted = true;

        async function fetchRobot() {
            try {
                const response = await fetch(`${apiBase}/id/${currentRobotId}`, {
                    headers: { 'Authorization': `Bearer ${token}` }
                });

                if (!response.ok) {
                    alert("Робора не знайдено");
                    if (isMounted) setLoading(false);
                    return;
                }

                const data = await response.json();

                if (isMounted) {
                    setRobot(data);
                    setEditName(data.roboName);
                    setEditStatus(data.status);
                    setLoading(false);
                }
            } catch (error) {
                console.error("Помилка завантаження:", error);
                if (isMounted) setLoading(false);
            }
        }

        fetchRobot();

        return () => {
            isMounted = false;
        };
    }, [currentRobotId, token, apiBase]);

    const saveEdit = async () => {
        const updateData = {
            roboId: currentRobotId,
            roboName: editName,
            status: editStatus,
            userId: robot.userId
        };

        try {
            const response = await fetch(`${apiBase}/edit`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(updateData)
            });

            if (response.ok) {
                alert("Дані оновлено!");
                setIsModalOpen(false);
                refreshData(currentRobotId);
            } else {
                alert("Помилка оновлення!");
            }
        } catch (error) {
            console.error("Помилка при збереженні:", error);
        }
    };

    const deleteThisRobot = async () => {
        if (!confirm("Ви впевнені, що хочете видалити цього робота?")) return;

        try {
            const response = await fetch(`${apiBase}/${currentRobotId}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (response.ok) {
                alert("Робора видалено!");
                let backUrl = '/robot-selection';
                if (ownerId) backUrl += `?ownerId=${ownerId}`;
                navigate(backUrl);
            } else {
                alert("Помилка при видаленні.");
            }
        } catch (error) {
            console.error("Помилка видалення:", error);
        }
    };

    const sendRobotCommand = async (action) => {
        if (robot.status === 'pending_activation') return;

        console.log(`Надсилаємо команду: ${action} для робота ${currentRobotId}`);
        try {
            await fetch(`${apiBase}/id/${currentRobotId}/control`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ action: action })
            });
        } catch (error) {
            console.error("Помилка зв'язку з сервером при керуванні:", error);
        }
    };

    if (loading) return <div className="container"><h2 className="welcome-guest-text">Завантаження...</h2></div>;
    if (!robot) return <div className="container"><h2 className="welcome-guest-text">Робора не знайдено</h2></div>;

    const qrConfigString = JSON.stringify({
        server_url: "https://localhost:7193",
        robot_id: robot.roboId,
        secret_token: robot.secretToken
    });

    return (
        <div className="container">
            <div className="main-grid">

                <div className="video-box">
                    {robot.status === 'pending_activation' ? (
                        <div className="form-container">
                            {!showQR ? (
                                <div>
                                    <p className="welcome-user-text">Робот ще не активований.</p>
                                    <button className="primary-btn" onClick={() => setShowQR(true)}>
                                        ⚙️ Створити QR-код підключення
                                    </button>
                                </div>
                            ) : (
                                <div>
                                    <p className="welcome-user-text">Покажіть цей код камері робота</p>
                                    <div>
                                        <QRCodeSVG value={qrConfigString} size={220} />
                                    </div>
                                    <p className="welcome-guest-text">ID: {robot.roboId}</p>
                                    <button className="btn-secondary" onClick={() => setShowQR(false)}>
                                        Назад
                                    </button>
                                </div>
                            )}
                        </div>
                    ) : (
                        <img
                            id="videoStream"
                            className="video-feed"
                            src={`${apiBase}/id/${robot.roboId}/stream`}
                            alt="Очікування сигналу з робота..."
                        />
                    )}
                </div>

                <div className="right-panel">
                    <div className="info-card">
                        <h2>🤖 Робот: <span className="welcome-user-text">{robot.roboName}</span></h2>
                        <p><strong>Статус:</strong> <span className="welcome-user-text">{robot.status}</span></p>
                        <p><strong>Створено:</strong> <span className="welcome-guest-text">{new Date(robot.createdAt).toLocaleDateString()}</span></p>
                    </div>

                    <div className={robot.status === 'pending_activation' ? "controls-container welcome-guest-text" : "controls-container"}>
                        <h3>Пульт керування</h3>
                        {robot.status === 'pending_activation' && <p>⚠️ Доступно після активації</p>}

                        <div className="remote-control-layout">
                            <div className="joystick-grid">
                                <button
                                    className="control-btn btn-up"
                                    disabled={robot.status === 'pending_activation'}
                                    onMouseDown={() => sendRobotCommand('forward')}
                                    onMouseUp={() => sendRobotCommand('stop')}
                                >⬆️</button>

                                <button
                                    className="control-btn btn-left"
                                    disabled={robot.status === 'pending_activation'}
                                    onMouseDown={() => sendRobotCommand('left')}
                                    onMouseUp={() => sendRobotCommand('stop')}
                                >⬅️</button>

                                <button
                                    className="control-btn btn-right"
                                    disabled={robot.status === 'pending_activation'}
                                    onMouseDown={() => sendRobotCommand('right')}
                                    onMouseUp={() => sendRobotCommand('stop')}
                                >➡️</button>

                                <button
                                    className="control-btn btn-down"
                                    disabled={robot.status === 'pending_activation'}
                                    onMouseDown={() => sendRobotCommand('backward')}
                                    onMouseUp={() => sendRobotCommand('stop')}
                                >⬇️</button>
                            </div>

                            <div className="camera-side-panel">
                                <span className="camera-label">Камера</span>
                                <button
                                    className="control-btn tilt-btn"
                                    disabled={robot.status === 'pending_activation'}
                                    onMouseDown={() => sendRobotCommand('cam-up')}
                                    onMouseUp={() => sendRobotCommand('stop')}
                                >🔼</button>
                                <button
                                    className="control-btn tilt-btn"
                                    disabled={robot.status === 'pending_activation'}
                                    onMouseDown={() => sendRobotCommand('cam-down')}
                                    onMouseUp={() => sendRobotCommand('stop')}
                                >🔽</button>
                            </div>
                        </div>
                    </div>

                    <div className="info-card">
                        <button className="edit-btn" onClick={() => setIsModalOpen(true)}>📝 Редагувати дані</button>
                        <button className="del-btn" onClick={deleteThisRobot}>🗑 Видалити робота</button>
                    </div>
                </div>
            </div>

            {isModalOpen && (
                <div className="modal">
                    <div className="modal-content">
                        <h3>Редагувати робота</h3>
                        <label>Назва:</label>
                        <input type="text" value={editName} onChange={(e) => setEditName(e.target.value)} />

                        <label>Статус:</label>
                        <input type="text" value={editStatus} onChange={(e) => setEditStatus(e.target.value)} />

                        <div>
                            <button className="primary-btn" onClick={saveEdit}>Зберегти</button>
                            <button className="btn-secondary" onClick={() => setIsModalOpen(false)}>Скасувати</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default RobotControl;