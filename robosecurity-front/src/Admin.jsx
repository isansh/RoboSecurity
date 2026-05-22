import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './App.css';

function Admin() {
    const navigate = useNavigate();
    const token = localStorage.getItem('token');
    const apiBase = 'https://localhost:7193/Users';

    const [users, setUsers] = useState([]);
    const [searchEmail, setSearchEmail] = useState('');

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editUserId, setEditUserId] = useState('');
    const [editEmail, setEditEmail] = useState('');
    const [editPassword, setEditPassword] = useState('');
    const [editConfirmPassword, setEditConfirmPassword] = useState('');
    const [editRoles, setEditRoles] = useState([]);

    useEffect(() => {
        if (!token) {
            navigate('/login');
        } else {
            loadAllUsers();
        }
    }, [token]);

    async function loadAllUsers() {
        try {
            const response = await fetch(apiBase, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (response.ok) {
                const data = await response.json();
                setUsers(data);
            } else {
                console.error("Не вдалося завантажити користувачів");
            }
        } catch (error) {
            console.error("Помилка мережі:", error);
        }
    }

    async function handleSearch() {
        if (!searchEmail) return alert("Введіть email");

        try {
            const response = await fetch(`${apiBase}/${searchEmail}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (response.ok) {
                const user = await response.json();
                setUsers([user]);
            } else {
                alert("Користувача не знайдено");
            }
        } catch
        {
            alert("Помилка пошуку");
        }
    }

    async function handleDeleteUser(id) {
        if (window.confirm("Видалити користувача?")) {
            try {
                const response = await fetch(`${apiBase}/${id}`, {
                    method: 'DELETE',
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                if (response.ok) {
                    loadAllUsers();
                }
            } catch
            {
                alert("Помилка при видаленні");
            }
        }
    }

    function openEditModal(user) {
        setEditUserId(user.userId);
        setEditEmail(user.userMail);
        setEditRoles(user.userRoles || []);
        setEditPassword('');
        setEditConfirmPassword('');
        setIsModalOpen(true);
    }

    function handleRoleChange(role) {
        if (editRoles.includes(role)) {
            setEditRoles(editRoles.filter(r => r !== role));
        } else {
            setEditRoles([...editRoles, role]);
        }
    }

    async function saveUserEdit() {
        if (editRoles.length === 0) {
            return alert("Користувач повинен мати хоча б одну роль!");
        }

        if (editPassword !== editConfirmPassword) {
            return alert("Паролі не співпадають!");
        }

        const data = {
            userId: parseInt(editUserId),
            userMail: editEmail,
            userRoles: editRoles,
            password: editPassword,
            confirmPassword: editConfirmPassword
        };

        try {
            const response = await fetch(`${apiBase}/edit`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data)
            });

            if (response.ok) {
                alert("Оновлено!");
                setIsModalOpen(false);
                loadAllUsers();
            } else {
                alert("Помилка оновлення");
            }
        } catch
        {
            alert("Помилка сети при обновлении");
        }
    }

    function viewUserRobots(userId) {
        navigate(`/robots?ownerId=${userId}`);
    }

    return (
        <div className="container">
            <h1>Управління користувачами</h1>

            <div id="search-section" style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <input
                    type="text"
                    placeholder="Введіть email для пошуку"
                    style={{ margin: 0, width: '300px' }}
                    value={searchEmail}
                    onChange={(e) => setSearchEmail(e.target.value)}
                />
                <button className="primary-btn" style={{ width: 'auto', margin: 0 }} onClick={handleSearch}>
                    Знайти
                </button>
                <button className="btn-secondary" onClick={loadAllUsers}>
                    Показати всіх
                </button>
            </div>

            <table id="usersTable">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Email</th>
                        <th>Ролі</th>
                        <th>Дії</th>
                    </tr>
                </thead>
                <tbody>
                    {users.map((u) => (
                        <tr key={u.userId}>
                            <td>{u.userId}</td>
                            <td>{u.userMail}</td>
                            <td>{u.userRoles && u.userRoles.length > 0 ? u.userRoles.join(', ') : 'немає ролей'}</td>
                            <td>
                                <button className="view-btn" onClick={() => viewUserRobots(u.userId)}>Роботи</button>
                                <button className="edit-btn" onClick={() => openEditModal(u)}>Редагувати</button>
                                <button className="del-btn" onClick={() => handleDeleteUser(u.userId)}>Видалити</button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>

            {isModalOpen && (
                <div className="modal" style={{ display: 'block' }}>
                    <div className="modal-content">
                        <h3>Редагувати користувача</h3>
                        <input type="text" placeholder="Email" value={editEmail} onChange={(e) => setEditEmail(e.target.value)} />

                        <label style={{ display: 'block', margin: '10px 0 5px', fontWeight: 'bold' }}>Ролі користувача:</label>
                        <div style={{ textAlign: 'left', marginLeft: '20px', marginBottom: '15px' }}>
                            <label>
                                <input type="checkbox" value="admin" checked={editRoles.includes('admin')} onChange={() => handleRoleChange('admin')} /> admin
                            </label><br />
                            <label>
                                <input type="checkbox" value="user" checked={editRoles.includes('user')} onChange={() => handleRoleChange('user')} /> user
                            </label><br />
                            <label>
                                <input type="checkbox" value="guard" checked={editRoles.includes('guard')} onChange={() => handleRoleChange('guard')} /> guard
                            </label>
                        </div>

                        <input type="password" placeholder="Новий пароль" value={editPassword} onChange={(e) => setEditPassword(e.target.value)} />
                        <input type="password" placeholder="Підтвердіть пароль" value={editConfirmPassword} onChange={(e) => setEditConfirmPassword(e.target.value)} />

                        <button className="primary-btn" onClick={saveUserEdit}>Зберегти</button>
                        <button className="btn-secondary" style={{ width: '100%', marginTop: '10px' }} onClick={() => setIsModalOpen(false)}>Скасувати</button>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Admin;