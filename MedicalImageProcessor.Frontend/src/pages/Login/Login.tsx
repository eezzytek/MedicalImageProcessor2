// components/LoginForm.tsx
import { useState } from "react";
import { useLogin } from "./useLogin.ts";
import { useAuth } from "../../contexts/AuthContext";
import { Link, useNavigate } from "react-router-dom";
import { Card, CardContent, TextField, Button, Typography, Box, Alert } from "@mui/material";

export const Login = () => {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const { loginUser, loading, error } = useLogin();
    const { user } = useAuth();
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        await loginUser({ username, password });
    };

    if (user) {
        navigate("/");
        return null;
    }

    return (
        <Box
            sx={{
                minHeight: "100vh",
                bgcolor: "#f5f7fa",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                p: 2
            }}
        >
            <Card sx={{ maxWidth: 420, width: "100%", boxShadow: 6, borderRadius: 4 }}>
                <CardContent sx={{ p: 5 }}>
                    <Typography variant="h4" component="h1" textAlign="center" gutterBottom color="primary" fontWeight="bold">
                        Medical Image Processor
                    </Typography>

                    <Typography variant="h5" textAlign="center" color="text.secondary" mb={4}>
                        Вхід в систему
                    </Typography>

                    {error && (
                        <Alert severity="error" sx={{ mb: 3 }}>
                            {error}
                        </Alert>
                    )}

                    <Box component="form" onSubmit={handleSubmit} sx={{ mt: 2 }}>
                        <TextField
                            fullWidth
                            label="Логін"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            margin="normal"
                            required
                            variant="outlined"
                        />
                        <TextField
                            fullWidth
                            label="Пароль"
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            margin="normal"
                            required
                            variant="outlined"
                        />

                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            size="large"
                            disabled={loading}
                            sx={{ mt: 4, py: 1.5, fontSize: "1.1rem", borderRadius: 3 }}
                        >
                            {loading ? "Вхід..." : "Увійти"}
                        </Button>
                    </Box>

                    <Typography textAlign="center" mt={3} color="text.secondary">
                        Немає акаунту?{" "}
                        <Link to="/register" style={{ color: "#1976d2", fontWeight: "bold" }}>
                            Зареєструватися
                        </Link>
                    </Typography>
                </CardContent>
            </Card>
        </Box>
    );
};
