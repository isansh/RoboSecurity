import './App.css';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

function Register() {
    const [email, setEmail] = useState('');
    const [phone, setPhoneNumber] = useState('');
    const [password, setPassword] = useState('');
    const [confPassword, setConfPassword] = useState('');
    const navigate = useNavigate();

    async function handleRegister() {
        if (phone && !/^\+380\d{9}$/.test(phone)) {
            return alert("Введіть номер у форматі +380XXXXXXXXX");
        }

        if (password !== confPassword) {
            return alert("Паролі не співпадають");
        }

        const regData = {
            UserMail: email,
            PhoneNumber: phone,
            Password: password,
            ConfirmPassword: confPassword,
            UserRoles: ["user"]
        };

        try {
            const response = await fetch('https://localhost:7193/Users', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(regData)
            });

            if (response.ok) {
                alert("Успіх!");
                navigate('/login');
            } else {
                const errorText = await response.text();
                alert(errorText || "Не вдалося створити акаунт");
            }
        }
        catch {
            alert("Помилка з'єднання із сервером");
        }
    }

    return (
        <div className="main-content">
            <div className="form-container">
                <h2>Реєстрація</h2>
                <input
                    type="email"
                    placeholder="Ваша пошта"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                />
                <input
                    type="tel"
                    placeholder="+380XXXXXXXXX"
                    value={phone}
                    onChange={(e) => setPhoneNumber(e.target.value)}
                />
                <input
                    type="password"
                    placeholder="Пароль"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                />
                <input
                    type="password"
                    placeholder="Підтвердження паролю"
                    value={confPassword}
                    onChange={(e) => setConfPassword(e.target.value)}
                />
                <button className="primary-btn" onClick={handleRegister}>
                    Створити акаунт
                </button>
            </div>
        </div>
    );
}

export default Register;