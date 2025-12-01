// src/App.tsx — ПОВНИЙ РОБОЧИЙ ДОДАТОК ДЛЯ ТВОГО C# API
import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import {
    Container, Box, Typography, TextField, Button, Card, CardContent,
    Alert, CircularProgress, Chip, Divider, LinearProgress
} from '@mui/material';
import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import AssessmentIcon from '@mui/icons-material/Assessment';
import LogoutIcon from '@mui/icons-material/Logout';

const API_URL = 'http://localhost:5077/api'; // ТВІЙ БЕКЕНД

function App() {
    const [token, setToken] = useState(localStorage.getItem('token') || '');
    const [user, setUser] = useState(localStorage.getItem('user') || '');
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    // Login
    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            const res = await fetch(`${API_URL}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });

            if (!res.ok) throw new Error('Invalid credentials');

            const data = await res.json();
            setToken(data.token);
            setUser(username);
            localStorage.setItem('token', data.token);
            localStorage.setItem('user', username);
        } catch (err) {
            setError('Login failed. Try doctor1 / password123');
        } finally {
            setLoading(false);
        }
    };

    const handleLogout = () => {
        setToken('');
        setUser('');
        localStorage.clear();
    };

    if (!token) {
        return (
            <Box sx={{
                minHeight: '100vh',
                bgcolor: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                p: 3
            }}>
                <Card sx={{ maxWidth: 400, width: '100%', p: 4 }}>
                    <Typography variant="h4" textAlign="center" mb={3}>
                        Medical Login
                    </Typography>
                    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

                    <Box component="form" onSubmit={handleLogin}>
                        <TextField
                            fullWidth
                            label="Username"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            margin="normal"
                            required
                        />
                        <TextField
                            fullWidth
                            label="Password"
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            margin="normal"
                            required
                        />
                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            size="large"
                            disabled={loading}
                            sx={{ mt: 3, py: 2 }}
                        >
                            {loading ? <CircularProgress size={24} /> : 'Sign In'}
                        </Button>
                    </Box>

                    <Typography variant="body2" color="text.secondary" textAlign="center" mt={3}>
                        Test: doctor1 / password123
                    </Typography>
                </Card>
            </Box>
        );
    }

    return (
        <Router>
            <Routes>
                <Route path="/" element={<Analyzer user={user} token={token} onLogout={handleLogout} />} />
                <Route path="*" element={<Navigate to="/" />} />
            </Routes>
        </Router>
    );
}

// ANALYZER PAGE
const Analyzer = ({ user, token, onLogout }: { user: string; token: string; onLogout: () => void }) => {
    const [file, setFile] = useState<File | null>(null);
    const [modelType, setModelType] = useState<'tumor' | 'fracture'>('tumor');
    const [loading, setLoading] = useState(false);
    const [result, setResult] = useState<any>(null);
    const [error, setError] = useState('');
    const [progress, setProgress] = useState(0);

    const handleAnalyze = async () => {
        if (!file) {
            setError('Please select an image first');
            return;
        }

        setLoading(true);
        setError('');
        setResult(null);

        const formData = new FormData();
        formData.append('imageFile', file);

        try {
            console.log('Sending request with token:', token?.substring(0, 20) + '...');

            const response = await fetch(
                `http://localhost:5001/api/Detection/detect?modelType=${modelType}`,
                {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        // НЕ ВКАЗУЙ Content-Type! Браузер сам поставить multipart/form-data з boundary
                    },
                    body: formData
                }
            );

            console.log('Response status:', response.status);

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Server error:', errorText);
                throw new Error(`Server error ${response.status}: ${errorText}`);
            }

            const data = await response.json();
            console.log('Success! Result:', data);

            setResult(data);
        } catch (err) {
            console.error('Analyze error:', err);
        } finally {
            setLoading(false);
        }
    };

    return (
        <Container maxWidth="md" sx={{ py: 4 }}>
            <Card sx={{ mb: 4 }}>
                <CardContent sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <div>
                        <Typography variant="h5">Welcome, {user}!</Typography>
                        <Typography variant="body2" color="text.secondary">
                            AI Medical Image Analysis
                        </Typography>
                    </div>
                    <Button variant="outlined" startIcon={<LogoutIcon />} onClick={onLogout}>
                        Logout
                    </Button>
                </CardContent>
            </Card>

            <Card>
                <CardContent>
                    <Typography variant="h5" gutterBottom textAlign="center">
                        <AssessmentIcon sx={{ fontSize: 40, verticalAlign: 'middle', mr: 1 }} />
                        Upload Medical Image
                    </Typography>

                    <Box sx={{ my: 4 }}>
                        <Box sx={{ display: 'flex', gap: 2, mb: 3, justifyContent: 'center' }}>
                            <Chip
                                label="🧠 Tumor Detection"
                                clickable
                                color={modelType === 'tumor' ? 'primary' : 'default'}
                                onClick={() => setModelType('tumor')}
                            />
                            <Chip
                                label="🦴 Fracture Detection"
                                clickable
                                color={modelType === 'fracture' ? 'primary' : 'default'}
                                onClick={() => setModelType('fracture')}
                            />
                        </Box>

                        <input
                            type="file"
                            accept="image/*"
                            onChange={(e) => e.target.files?.[0] && setFile(e.target.files[0])}
                            style={{ display: 'none' }}
                            id="upload"
                        />
                        <label htmlFor="upload">
                            <Button
                                component="span"
                                variant="outlined"
                                startIcon={<CloudUploadIcon />}
                                fullWidth
                                size="large"
                            >
                                {file ? file.name : 'Choose Image'}
                            </Button>
                        </label>
                    </Box>

                    <Button
                        variant="contained"
                        size="large"
                        fullWidth
                        onClick={handleAnalyze}
                        disabled={!file || loading}
                        startIcon={loading ? <CircularProgress size={24} /> : <AssessmentIcon />}
                        sx={{ py: 2 }}
                    >
                        {loading ? 'Analyzing...' : 'Start AI Analysis'}
                    </Button>

                    {loading && (
                        <Box sx={{ mt: 3 }}>
                            <LinearProgress variant="determinate" value={progress} />
                        </Box>
                    )}

                    {error && <Alert severity="error" sx={{ mt: 3 }}>{error}</Alert>}

                    {result && (
                        <Box sx={{ mt: 4 }}>
                            <Typography variant="h6" gutterBottom textAlign="center">
                                Analysis Results
                            </Typography>
                            <Divider sx={{ my: 2 }} />

                            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 3 }}>
                                <Card>
                                    <CardContent>
                                        <Typography variant="h6" color="primary">🧠 Brain Tumor</Typography>
                                        <Typography variant="h3">
                                            {Math.round(result.brainTumorConfidence * 100)}%
                                        </Typography>
                                        <Chip
                                            label={result.hasBrainTumor ? 'DETECTED' : 'NORMAL'}
                                            color={result.hasBrainTumor ? 'error' : 'success'}
                                        />
                                    </CardContent>
                                </Card>

                                <Card>
                                    <CardContent>
                                        <Typography variant="h6" color="secondary">🦴 Fracture</Typography>
                                        <Typography variant="h3">
                                            {Math.round(result.fractureConfidence * 100)}%
                                        </Typography>
                                        <Chip
                                            label={result.hasFracture ? 'DETECTED' : 'NORMAL'}
                                            color={result.hasFracture ? 'error' : 'success'}
                                        />
                                    </CardContent>
                                </Card>
                            </Box>
                        </Box>
                    )}
                </CardContent>
            </Card>
        </Container>
    );
};

export default App;