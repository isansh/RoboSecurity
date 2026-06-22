import { useState, useEffect, useRef } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { QRCodeSVG } from 'qrcode.react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import './App.css';

const API_CONFIG = {
    BASE_URL: `${import.meta.env.VITE_API_URL}/Robots`,
    HUB_URL: `${import.meta.env.VITE_API_URL}/robotHub`
};

const RobotControl = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    const currentRobotId = searchParams.get('id');
    const ownerId = searchParams.get('ownerId');
    const token = localStorage.getItem('token');

    const [robot, setRobot] = useState(null);
    const [loading, setLoading] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [showQR, setShowQR] = useState(false);

    const [editName, setEditName] = useState('');
    const [editStatus, setEditStatus] = useState('');
    const [videoSrc, setVideoSrc] = useState(null);

    const hubConnectionRef = useRef(null);

    const lastSentActionRef = useRef('stop');

    const isWatchdogActive = robot && robot.status === 'watchdog';
    const [isLightOn, setIsLightOn] = useState(false);

    const ROBOT_STATUSES = {
        'pending_activation': { text: 'Очікує підключення', className: 'status-pending' },
        'active': { text: 'Онлайн', className: 'status-online' },
        'watchdog': { text: 'Режим охорони', className: 'status-watchdog' },
        'offline': { text: 'Офлайн', className: 'status-offline' }
    };

    const refreshData = async (id) => {
        try {
            const response = await fetch(`${API_CONFIG.BASE_URL}/id/${id}`, {
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
                const response = await fetch(`${API_CONFIG.BASE_URL}/id/${currentRobotId}`, {
                    headers: { 'Authorization': `Bearer ${token}` }
                });

                if (!response.ok) {
                    alert("Робота не знайдено");
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
    }, [currentRobotId, token]);

    useEffect(() => {
        if (!robot || robot.status === 'pending_activation') return;

        const connection = new HubConnectionBuilder()
            .withUrl(API_CONFIG.HUB_URL, {
                accessTokenFactory: () => localStorage.getItem("token")
            })
            .withAutomaticReconnect()
            .build();

        async function startSignalR() {
            try {
                await connection.start();
                console.log("SignalR підключено. Готовий до трансляції та керування.");

                hubConnectionRef.current = connection;

                await connection.invoke("StartWatchingRobot", parseInt(currentRobotId));

                connection.on("ReceiveFrame", (frameBytes) => {
                    setVideoSrc(`data:image/jpeg;base64,${frameBytes}`);
                });

            } catch (err) {
                console.error("Помилка старту SignalR:", err);
            }
        }

        startSignalR();

        return () => {
            if (connection) {
                connection.invoke("StopWatchingRobot", parseInt(currentRobotId))
                    .then(() => connection.stop())
                    .catch(err => console.error("Помилка відключення сокета:", err));
            }
        };
    }, [robot, currentRobotId]);

    const sendRobotCommand = async (action) => {
        if (!robot || robot.status === 'pending_activation') return;

        const isCameraOrLight = ['cam-up', 'cam-down', 'lights-on', 'lights-off'].includes(action);

        if (!isCameraOrLight && action === lastSentActionRef.current) {
            return;
        }

        lastSentActionRef.current = action;
        console.log(`Фронтенд відправляє унікальну команду: ${action}`);

        try {
            await fetch(`${API_CONFIG.BASE_URL}/id/${currentRobotId}/control?action=${action}`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${token}` }
            });
        } catch (error) {
            console.error("Помилка відправки команди:", error);
            lastSentActionRef.current = '';
        }
    };

    const handleButtonPress = (action) => {
        sendRobotCommand(action);
    };

    const handleButtonRelease = () => {
        sendRobotCommand('stop');
    };

    const saveEdit = async () => {
        const updateData = {
            roboId: currentRobotId,
            roboName: editName,
            status: editStatus,
            userId: robot.userId,
        };

        try {
            const response = await fetch(`${API_CONFIG.BASE_URL}/edit`, {
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
            const response = await fetch(`${API_CONFIG.BASE_URL}/${currentRobotId}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (response.ok) {
                alert("Робота видалено!");
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

    const toggleWatchdogMode = async () => {
        if (robot.status === 'pending_activation') return;

        const nextStatusInDb = isWatchdogActive ? 'active' : 'watchdog';
        const actionCommand = isWatchdogActive ? 'watchdog-stop' : 'watchdog';

        const updateData = {
            roboId: currentRobotId,
            roboName: robot.roboName,
            status: nextStatusInDb,
            userId: robot.userId,
        };

        try {
            const editResponse = await fetch(`${API_CONFIG.BASE_URL}/edit`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(updateData)
            });

            if (editResponse.ok) {
                sendRobotCommand(actionCommand);
                refreshData(currentRobotId);
            } else {
                alert("Помилка оновлення статусу охорони.");
            }
        } catch (error) {
            console.error("Помилка охорони:", error);
        }
    };

    const handleLightClick = () => {
        const nextState = !isLightOn;
        setIsLightOn(nextState);
        sendRobotCommand(nextState ? 'lights-on' : 'lights-off');
    };

    if (loading) return <div className="container"><h2 className="welcome-guest-text">Завантаження...</h2></div>;
    if (!robot) return <div className="container"><h2 className="welcome-guest-text">Робора не знайдено</h2></div>;

    const qrConfigString = JSON.stringify({
        server_url: API_CONFIG.HUB_URL,
        secret_token: robot.token
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
                                    <p className="welcome-guest-text">Очікування підключення пристрою...</p>
                                    <button className="btn-secondary" onClick={() => setShowQR(false)}>Назад</button>
                                </div>
                            )}
                        </div>
                    ) : (
                        <div className="video-stream-container">
                            {videoSrc ? (
                                <img src={videoSrc} alt="Robot Live" className="robot-video-feed" />
                            ) : (
                                <div className="video-placeholder">
                                    <p>📭 Очікуємо перші кадри від робота...</p>
                                </div>
                            )}
                        </div>
                    )}
                </div>

                <div className="right-panel">
                    <div className="info-card">
                        <h2>🤖 Робот: <span className="welcome-user-text">{robot.roboName}</span></h2>
                        <p>
                            <strong>Статус:</strong>{' '}
                            <span className={`status-badge ${(ROBOT_STATUSES[robot.status] || {}).className || ''}`}>
                                {(ROBOT_STATUSES[robot.status] || {}).text || robot.status}
                            </span>
                        </p>
                        <p><strong>Створено:</strong> <span className="welcome-guest-text">{new Date(robot.createdAt).toLocaleDateString()}</span></p>
                    </div>

                    <div className="watchdog-panel">
                        <button
                            className={`control-btn watchdog-btn ${isWatchdogActive ? 'watchdog-active' : 'watchdog-inactive'}`}
                            disabled={robot.status === 'pending_activation'}
                            onClick={toggleWatchdogMode}
                        >
                            {isWatchdogActive ? '🚨 Охорона АКТИВНА (Вимкнути)' : '🛡️ Увімкнути охорону'}
                        </button>
                    </div>

                    <div className="controls-container">
                        <h3>Пульт керування</h3>
                        {robot.status === 'pending_activation' && <p>⚠️ Доступно після активації</p>}

                        <div className="remote-control-layout">
                            <div className="joystick-grid" onMouseLeave={handleButtonRelease}>
                                <button
                                    className="control-btn btn-up"
                                    disabled={robot.status === 'pending_activation'}
                                    onMouseDown={() => handleButtonPress('forward')}
                                    onMouseUp={handleButtonRelease}
                                    onTouchStart={() => handleButtonPress('forward')}
                                    onTouchEnd={handleButtonRelease}
                                >⬆️</button>

                                <button
                                    className="control-btn btn-left"
                                    disabled={robot.status === 'pending_activation'}
                                    onMouseDown={() => handleButtonPress('left')}
                                    onMouseUp={handleButtonRelease}
                                    onTouchStart={() => handleButtonPress('left')}
                                    onTouchEnd={handleButtonRelease}
                                >⬅️</button>

                                <button
                                    className="control-btn btn-right"
                                    disabled={robot.status === 'pending_activation'}
                                    onMouseDown={() => handleButtonPress('right')}
                                    onMouseUp={handleButtonRelease}
                                    onTouchStart={() => handleButtonPress('right')}
                                    onTouchEnd={handleButtonRelease}
                                >➡️</button>

                                <button
                                    className="control-btn btn-down"
                                    disabled={robot.status === 'pending_activation'}
                                    onMouseDown={() => handleButtonPress('backward')}
                                    onMouseUp={handleButtonRelease}
                                    onTouchStart={() => handleButtonPress('backward')}
                                    onTouchEnd={handleButtonRelease}
                                >⬇️</button>
                            </div>

                            <div className="camera-side-panel">
                                <span className="camera-label">Камера</span>
                                <button
                                    className="control-btn tilt-btn"
                                    disabled={robot.status === 'pending_activation'}
                                    onClick={() => sendRobotCommand('cam-up')}
                                >🔼</button>

                                <button
                                    className="control-btn tilt-btn"
                                    disabled={robot.status === 'pending_activation'}
                                    onClick={() => sendRobotCommand('cam-down')}
                                >🔽</button>
                            </div>

                            <button
                                className={`control-btn light-btn ${isLightOn ? 'light-active' : 'light-inactive'}`}
                                disabled={robot.status === 'pending_activation'}
                                onClick={handleLightClick}
                            >
                                {isLightOn ? '💡' : '🔌'}
                            </button>
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