import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './App.css';

function Admin() {
    const navigate = useNavigate();
    const token = localStorage.getItem('token');
    const apiBase = 'https://localhost:7193/Users';

    const [users, setUsers] = useState([]);
    const [searchEmail, setSearchEmail] = useState('');

    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [editUserId, setEditUserId] = useState('');
    const [editEmail, setEditEmail] = useState('');
    const [editPhone, setEditPhone] = useState('');
    const [editPassword, setEditPassword] = useState('');
    const [editConfirmPassword, setEditConfirmPassword] = useState('');
    const [editRoles, setEditRoles] = useState([]);

    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [createEmail, setCreateEmail] = useState('');
    const [createPhone, setCreatePhone] = useState('');
    const [createPassword, setCreatePassword] = useState('');
    const [createConfirmPassword, setCreateConfirmPassword] = useState('');
    const [createRoles, setCreateRoles] = useState(['user']);

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
        } catch {
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
            } catch {
                alert("Помилка при видаленні");
            }
        }
    }

    function openEditModal(user) {
        setEditUserId(user.userId);
        setEditEmail(user.userMail);
        setEditPhone(user.userPhone || user.phoneNumber || '');
        setEditRoles(user.userRoles || []);
        setEditPassword('');
        setEditConfirmPassword('');
        setIsEditModalOpen(true);
    }

    function handleEditRoleChange(role) {
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

        if (editPhone && !/^\+380\d{9}$/.test(editPhone)) {
            return alert("Введіть номер у форматі +380XXXXXXXXX");
        }

        const data = {
            UserId: parseInt(editUserId),
            UserMail: editEmail,
            PhoneNumber: editPhone,
            UserRoles: editRoles,
            Password: editPassword,
            ConfirmPassword: editConfirmPassword
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
                alert("Оновлено успішно!");
                setIsEditModalOpen(false);
                loadAllUsers();
            } else {
                alert("Помилка оновлення. Перевірте формат даних.");
            }
        } catch {
            alert("Помилка мережі при оновленні");
        }
    }

    function openCreateModal() {
        setCreateEmail('');
        setCreatePhone('');
        setCreatePassword('');
        setCreateConfirmPassword('');
        setCreateRoles(['user']);
        setIsCreateModalOpen(true);
    }

    function handleCreateRoleChange(role) {
        if (createRoles.includes(role)) {
            setCreateRoles(createRoles.filter(r => r !== role));
        } else {
            setCreateRoles([...createRoles, role]);
        }
    }

    async function handleCreateUser() {
        if (!createEmail || !createPassword) {
            return alert("Email та Пароль є обов'язковими!");
        }
        if (createRoles.length === 0) {
            return alert("Оберіть хоча б одну роль!");
        }
        if (createPassword !== createConfirmPassword) {
            return alert("Паролі не співпадають!");
        }

        if (createPhone && !/^\+380\d{9}$/.test(createPhone)) {
            return alert("Введіть номер у форматі +380XXXXXXXXX");
        }

        const regData = {
            UserMail: createEmail,
            PhoneNumber: createPhone,
            Password: createPassword,
            ConfirmPassword: createConfirmPassword,
            UserRoles: createRoles
        };

        try {
            const response = await fetch(apiBase, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(regData)
            });

            if (response.ok) {
                alert("Користувача успішно створено!");
                setIsCreateModalOpen(false);
                loadAllUsers();
            } else {
                const serverError = await response.text();
                alert(serverError || "Не вдалося створити користувача.");
            }
        } catch {
            alert("Помилка мережі при створенні користувача");
        }
    }

    function viewUserRobots(userId) {
        navigate(`/robots?ownerId=${userId}`);
    }

    return (
        <div className="container">
            <h1>Управління користувачами</h1>

            <div id="search-section">
                <input
                    type="text"
                    placeholder="Введіть email для пошуку"
                    className="search-input"
                    value={searchEmail}
                    onChange={(e) => setSearchEmail(e.target.value)}
                />
                <button className="primary-btn btn-auto-width" onClick={handleSearch}>
                    Знайти
                </button>
                <button className="btn-secondary btn-auto-width" onClick={loadAllUsers}>
                    Показати всіх
                </button>

                <button className="edit-btn btn-create-user" onClick={openCreateModal}>
                    ➕ Створити користувача
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
                            <td><b>#{u.userId}</b></td>
                            <td className="welcome-user-text">{u.userMail}</td>
                            <td>{u.userRoles && u.userRoles.length > 0 ? u.userRoles.join(', ') : <span className="welcome-guest-text">немає ролей</span>}</td>
                            <td>
                                <button
                                    className="view-btn"
                                    onClick={() => viewUserRobots(u.userId)}
                                    style={{
                                        visibility: u.userRoles?.includes('user') ? 'visible' : 'hidden'
                                    }}
                                >
                                    Роботи
                                </button>
                                <button className="edit-btn" onClick={() => openEditModal(u)}>Редагувати</button>
                                <button className="del-btn" onClick={() => handleDeleteUser(u.userId)}>Видалити</button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>

            {isEditModalOpen && (
                <div className="modal modal-scrollable">
                    <div className="modal-content modal-content-scrollable">
                        <h3>Редагувати користувача</h3>
                        <input type="text" placeholder="Email" value={editEmail} onChange={(e) => setEditEmail(e.target.value)} />
                        <input type="text" placeholder="Номер телефону" value={editPhone} onChange={(e) => setEditPhone(e.target.value)} />

                        <label className="modal-section-label">Ролі користувача:</label>
                        <div className="modal-checkbox-group">
                            <label className="checkbox-container">
                                <input type="checkbox" value="admin" checked={editRoles.includes('admin')} className="checkbox-input" onChange={() => handleEditRoleChange('admin')} /> admin
                            </label>
                            <label className="checkbox-container">
                                <input type="checkbox" value="user" checked={editRoles.includes('user')} className="checkbox-input" onChange={() => handleEditRoleChange('user')} /> user
                            </label>
                            <label className="checkbox-container">
                                <input type="checkbox" value="guard" checked={editRoles.includes('guard')} className="checkbox-input" onChange={() => handleEditRoleChange('guard')} /> guard
                            </label>
                        </div>

                        <input type="password" placeholder="Новий пароль" value={editPassword} onChange={(e) => setEditPassword(e.target.value)} />
                        <input type="password" placeholder="Підтвердіть пароль" value={editConfirmPassword} onChange={(e) => setEditConfirmPassword(e.target.value)} />

                        <button className="primary-btn" onClick={saveUserEdit}>Зберегти</button>
                        <button className="btn-secondary" onClick={() => setIsEditModalOpen(false)}>Скасувати</button>
                    </div>
                </div>
            )}

            {isCreateModalOpen && (
                <div className="modal modal-scrollable">
                    <div className="modal-content modal-content-scrollable">
                        <h3>Створити нового користувача</h3>
                        <input type="text" placeholder="Email (Логін)" value={createEmail} onChange={(e) => setCreateEmail(e.target.value)} />
                        <input type="tel" placeholder="+380XXXXXXXXX" pattern="^\+380\d{9}$" value={createPhone} onChange={(e) => setCreatePhone(e.target.value)} />
                        <label className="modal-section-label">Призначити ролі:</label>
                        <div className="modal-checkbox-group">
                            <label className="checkbox-container">
                                <input type="checkbox" value="admin" checked={createRoles.includes('admin')} className="checkbox-input" onChange={() => handleCreateRoleChange('admin')} /> admin
                            </label>
                            <label className="checkbox-container">
                                <input type="checkbox" value="user" checked={createRoles.includes('user')} className="checkbox-input" onChange={() => handleCreateRoleChange('user')} /> user
                            </label>
                            <label className="checkbox-container">
                                <input type="checkbox" value="guard" checked={createRoles.includes('guard')} className="checkbox-input" onChange={() => handleCreateRoleChange('guard')} /> guard
                            </label>
                        </div>

                        <input type="password" placeholder="Пароль" value={createPassword} onChange={(e) => setCreatePassword(e.target.value)} />
                        <input type="password" placeholder="Підтвердіть пароль" value={createConfirmPassword} onChange={(e) => setCreateConfirmPassword(e.target.value)} />

                        <button className="primary-btn" onClick={handleCreateUser}>Створити</button>
                        <button className="btn-secondary" onClick={() => setIsCreateModalOpen(false)}>Скасувати</button>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Admin;