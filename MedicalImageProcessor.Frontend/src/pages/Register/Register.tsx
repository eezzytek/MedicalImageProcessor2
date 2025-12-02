// components/RegisterForm.tsx
import { useState } from "react";
import { useRegister } from "./useRegister.ts";
import { useAuth } from "../../contexts/AuthContext";
import { Link, useNavigate } from "react-router-dom";
import { Card, CardContent, TextField, Button, Typography, Box, Alert } from "@mui/material";

export const Register = () => {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const { registerUser, loading, error } = useRegister();
    const { user } = useAuth();
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (password !== confirmPassword) {
            alert("Паролі не співпадають!");
            return;
        }
        await registerUser({ username, password });
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
                        Реєстрація
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
                        />
                        <TextField
                            fullWidth
                            label="Пароль"
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            margin="normal"
                            required
                        />
                        <TextField
                            fullWidth
                            label="Повторіть пароль"
                            type="password"
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                            margin="normal"
                            required
                        />

                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            size="large"
                            disabled={loading}
                            sx={{ mt: 4, py: 1.5, fontSize: "1.1rem", borderRadius: 3 }}
                        >
                            {loading ? "Реєстрація..." : "Зареєструватися"}
                        </Button>
                    </Box>

                    <Typography textAlign="center" mt={3} color="text.secondary">
                        Вже є акаунт?{" "}
                        <Link to="/login" style={{ color: "#1976d2", fontWeight: "bold" }}>
                            Увійти
                        </Link>
                    </Typography>
                </CardContent>
            </Card>
        </Box>
    );
};