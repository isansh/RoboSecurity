import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

const getPayload = (token) => {
    if (!token) return null;
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        return JSON.parse(window.atob(base64));
    } catch {
        return null;
    }
};

function Header() {
    const navigate = useNavigate();

    const handleLogout = () => {
        localStorage.removeItem('token');
        setIsLoggedIn(false);
        setUserRoles([]);
        navigate('/');
    };

    const [isLoggedIn, setIsLoggedIn] = useState(() => {
        return !!localStorage.getItem('token');
    });

    const [userRoles, setUserRoles] = useState(() => {
        const token = localStorage.getItem('token');
        const payload = getPayload(token);
        if (payload) {
            const roles = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload.role || [];
            return Array.isArray(roles) ? roles.map(r => r.toLowerCase()) : [roles.toLowerCase()];
        }
        return [];
    });

    useEffect(() => {
        const token = localStorage.getItem('token');
        if (token && !getPayload(token)) {
            handleLogout();
        }
    }, []);

    const isAdmin = userRoles.includes('admin');
    const isGuard = userRoles.includes('guard');
    const isUser = userRoles.includes('user') || userRoles.length === 0;

    return (
        <header>
            <a href="/" className="logo">RoboSecurity</a>
            <div className="nav-links">
                {!isLoggedIn ? (
                    <>
                        <button className="btn-light-blue" onClick={() => navigate('/login')}>Увійти</button>
                        <button className="btn-light-pink" onClick={() => navigate('/register')}>Реєстрація</button>
                    </>
                ) : (
                    <>
                        {isAdmin && (
                            <button className="btn-light-blue" onClick={() => navigate('/admin')}>Усі користувачі</button>
                        )}

                        {isGuard && (
                            <button className="btn-light-blue" style={{ backgroundColor: '#e2f0d9', color: '#385723' }} onClick={() => navigate('/guard')}>
                                🚨 Сповіщення
                            </button>
                        )}

                        {isUser && (
                            <button className="btn-light-blue" onClick={() => navigate('/robots')}>Мої роботи</button>
                        )}

                        <button className="btn-logout" onClick={handleLogout}>Вихід</button>
                    </>
                )}
            </div>
        </header>
    );
}

export default Header;