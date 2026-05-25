import './App.css';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

function Login() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const navigate = useNavigate();
    async function handleLogin() {
        const loginData = {
            UserMail: email,
            UserPassword: password
        };

        try {
            const response = await fetch('https://localhost:7193/Login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(loginData)
            });

            if (response.ok) {
                const result = await response.json();

                localStorage.setItem('token', result.token);
                localStorage.setItem('roles', JSON.stringify(result.userRoles));

                if (result.userRoles.includes('admin')) {
                    navigate('/admin');
                } else if (result.userRoles.includes('guard')) {
                    navigate('/guard');
                } else {
                    navigate('/robots');
                }

                window.location.reload();

            } else {
                alert("Помилка входу: перевірте пошту чи пароль");
            }
        } catch {
            alert("Не вдалося з'єднатися з сервером");
        }
    }

    return (
        <div className="main-content">
            <div className="form-container">
                <h2>Вхід</h2>
                <input
                    type="email"
                    placeholder="Ваша пошта"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                />
                <input
                    type="password"
                    placeholder="Пароль"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                />
                <button className="primary-btn" onClick={handleLogin}>
                    Увійти
                </button>
            </div>
        </div>
    );
}

export default Login;
