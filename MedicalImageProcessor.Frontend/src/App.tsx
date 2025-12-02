import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { Login } from './pages/Login/Login';
import { Register } from './pages/Register/Register';
import { Navbar } from './components/Navbar/Navbar';
import { ProtectedRoute } from './components/ProtectedRoute/ProtectedRoute';
import { Results } from './pages/Results/Results';
import {DetectionImageInput} from "./pages/DetectionImageInput/DetectionImageInput"
// Example page components
const Home: React.FC = () => <h2>Home Page</h2>;

const App: React.FC = () => {
  return (
      <AuthProvider>
      <Navbar/>
    <Router>

      <Routes>
        <Route path="/" element={<ProtectedRoute><Results /></ProtectedRoute>} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path = "/detection" element = {<DetectionImageInput/>}/>
      </Routes>
    </Router>
    </AuthProvider>
  );
};

export default App;
