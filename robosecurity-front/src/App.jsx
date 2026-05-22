import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Header from './Header';
import Home from './Home';
import Login from './Login';
import Register from './Register';
import Admin from './Admin';
import Robots from './Robots';
import Guard from './Guard';
import './App.css';

function App() {
    return (
        <Router>
            <div className="app-container">
                <Header />

                <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/admin" element={<Admin />} />
                    <Route path="/robots" element={<Robots />} />
                    <Route path="/guard" element={<Guard />} />
                </Routes>
            </div>
        </Router>
    );
}

export default App;