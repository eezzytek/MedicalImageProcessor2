// pages/DetectionImageInput.tsx
import React, { useState, useRef, useCallback } from "react";
import { useAuth } from "../../contexts/AuthContext";
import "./DetectionImageInput.css";
import {
    Box,
    Button,
    Card,
    CardContent,
    Typography,
    Alert,
    CircularProgress,
    Chip,
    LinearProgress,
    IconButton,
    Tooltip
} from "@mui/material";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import AssessmentIcon from "@mui/icons-material/Assessment";

export const DetectionImageInput: React.FC = () => {
    const { user } = useAuth();
    const [image, setImage] = useState<File | null>(null);
    const [imageUrl, setImageUrl] = useState<string | null>(null);
    const [modelType, setModelType] = useState<"tumor" | "fracture">("tumor");
    const [result, setResult] = useState<any>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [copied, setCopied] = useState(false);
    const [dragActive, setDragActive] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);

    // Обробка Drag & Drop
    const handleDrag = (e: React.DragEvent) => {
        e.preventDefault();
        e.stopPropagation();
        if (e.type === "dragenter" || e.type === "dragover") {
            setDragActive(true);
        } else if (e.type === "dragleave") {
            setDragActive(false);
        }
    };

    const handleDrop = (e: React.DragEvent) => {
        e.preventDefault();
        e.stopPropagation();
        setDragActive(false);

        if (e.dataTransfer.files && e.dataTransfer.files[0]) {
            handleFile(e.dataTransfer.files[0]);
        }
    };

    // Обробка вибору файлу
    const handleFile = (file: File) => {
        if (file.type.startsWith("image/")) {
            setImage(file);
            setImageUrl(URL.createObjectURL(file));
            setError(null);
            setResult(null);
        } else {
            setError("Будь ласка, виберіть зображення");
        }
    };

    // Вставка з буфера (Ctrl+V)
    const handlePaste = useCallback((e: ClipboardEvent) => {
        const items = e.clipboardData?.items;
        if (!items) return;

        for (let i = 0; i < items.length; i++) {
            if (items[i].type.indexOf("image") !== -1) {
                const blob = items[i].getAsFile();
                if (blob) {
                    const file = new File([blob], "pasted-image.png", { type: blob.type });
                    handleFile(file);
                }
                break;
            }
        }
    }, []);

    React.useEffect(() => {
        document.addEventListener("paste", handlePaste);
        return () => document.removeEventListener("paste", handlePaste);
    }, [handlePaste]);

    // Копіювання посилання
    const copyImageUrl = () => {
        if (imageUrl) {
            navigator.clipboard.writeText(imageUrl);
            setCopied(true);
            setTimeout(() => setCopied(false), 2000);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!image) {
            setError("Будь ласка, виберіть зображення");
            return;
        }

        setLoading(true);
        setError(null);
        setResult(null);

        try {
            const formData = new FormData();
            formData.append("imageFile", image);

            const url = `${import.meta.env.VITE_API_URL}/api/Detection/detect?modelType=${modelType}`;

            const response = await fetch(url, {
                method: "POST",
                headers: {
                    Authorization: `Bearer ${user?.token}`,
                },
                body: formData,
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData?.message || "Detection failed");
            }

            const data = await response.json();
            setResult(data);
        } catch (err: any) {
            setError(err.message || "Щось пішло не так");
        } finally {
            setLoading(false);
        }
    };

    return (
        <Box sx={{ maxWidth: 800, mx: "auto", p: 3 }}>
            <Typography variant="h4" component="h1" gutterBottom textAlign="center" color="primary">
                🩺 Аналіз медичних зображень
            </Typography>

            <Card sx={{ mb: 4, p: 3 }}>
                <CardContent>
                    <Typography variant="h6" gutterBottom>
                        Тип аналізу
                    </Typography>
                    <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap", mb: 3 }}>
                        <Chip
                            label="🧠 Пухлина мозку (MRI)"
                            clickable
                            color={modelType === "tumor" ? "primary" : "default"}
                            onClick={() => setModelType("tumor")}
                            variant={modelType === "tumor" ? "filled" : "outlined"}
                        />
                        <Chip
                            label="🦴 Переломи (Рентген)"
                            clickable
                            color={modelType === "fracture" ? "primary" : "default"}
                            onClick={() => setModelType("fracture")}
                            variant={modelType === "fracture" ? "filled" : "outlined"}
                        />
                    </Box>

                    {/* Drag & Drop + Paste Area */}
                    <Box
                        onDragEnter={handleDrag}
                        onDragLeave={handleDrag}
                        onDragOver={handleDrag}
                        onDrop={handleDrop}
                        sx={{
                            border: "3px dashed",
                            borderColor: dragActive ? "primary.main" : "#ccc",
                            borderRadius: 3,
                            p: 4,
                            textAlign: "center",
                            bgcolor: dragActive ? "primary.50" : "grey.50",
                            transition: "all 0.3s ease",
                            cursor: "pointer",
                            mb: 3
                        }}
                    >
                        <input
                            ref={fileInputRef}
                            type="file"
                            accept="image/*"
                            onChange={(e) => e.target.files?.[0] && handleFile(e.target.files[0])}
                            style={{ display: "none" }}
                        />

                        {imageUrl ? (
                            <Box sx={{ position: "relative", display: "inline-block" }}>
                                <img
                                    src={imageUrl}
                                    alt="Uploaded"
                                    style={{
                                        maxWidth: "100%",
                                        maxHeight: 400,
                                        borderRadius: 12,
                                        boxShadow: "0 8px 25px rgba(0,0,0,0.15)"
                                    }}
                                />
                                <Tooltip title={copied ? "Скопійовано!" : "Копіювати посилання"}>
                                    <IconButton
                                        onClick={copyImageUrl}
                                        sx={{
                                            position: "absolute",
                                            top: 10,
                                            right: 10,
                                            bgcolor: "background.paper",
                                            "&:hover": { bgcolor: "grey.200" }
                                        }}
                                    >
                                        {copied ? <CheckCircleIcon color="success" /> : <ContentCopyIcon />}
                                    </IconButton>
                                </Tooltip>
                            </Box>
                        ) : (
                            <>
                                <CloudUploadIcon sx={{ fontSize: 80, color: "grey.400", mb: 2 }} />
                                <Typography variant="h6" gutterBottom>
                                    Перетягніть зображення сюди або натисніть для вибору
                                </Typography>
                                <Typography variant="body2" color="text.secondary">
                                    Або вставте з буфера обміну (Ctrl+V)
                                </Typography>
                                <Button
                                    variant="contained"
                                    onClick={() => fileInputRef.current?.click()}
                                    sx={{ mt: 3 }}
                                >
                                    Вибрати файл
                                </Button>
                            </>
                        )}
                    </Box>

                    {image && (
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                            Вибрано: <strong>{image.name}</strong> ({(image.size / 1024 / 1024).toFixed(2)} MB)
                        </Typography>
                    )}

                    <Button
                        variant="contained"
                        size="large"
                        fullWidth
                        onClick={handleSubmit}
                        disabled={!image || loading}
                        startIcon={loading ? <CircularProgress size={24} /> : <AssessmentIcon />}
                        sx={{ py: 2, fontSize: "1.1rem" }}
                    >
                        {loading ? "Аналіз..." : "Запустити AI аналіз"}
                    </Button>

                    {loading && (
                        <Box sx={{ mt: 3 }}>
                            <LinearProgress />
                            <Typography variant="body2" color="text.secondary" sx={{ mt: 1, textAlign: "center" }}>
                                Нейронна мережа обробляє зображення...
                            </Typography>
                        </Box>
                    )}

                    {error && (
                        <Alert severity="error" sx={{ mt: 3 }}>
                            {error}
                        </Alert>
                    )}

                    {result && (
                        <Card sx={{ mt: 4 }}>
                            <CardContent>
                                <Typography variant="h5" gutterBottom textAlign="center">
                                    Результат аналізу
                                </Typography>
                                <Box sx={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 3 }}>
                                    <Card variant="outlined">
                                        <CardContent>
                                            <Typography variant="h6" color="primary">
                                                🧠 Пухлина мозку
                                            </Typography>
                                            <Typography variant="h3">
                                                {Math.round(result.brainTumorConfidence * 100)}%
                                            </Typography>
                                            <Chip
                                                label={result.hasBrainTumor ? "ВИЯВЛЕНО" : "НОРМА"}
                                                color={result.hasBrainTumor ? "error" : "success"}
                                                size="small"
                                            />
                                        </CardContent>
                                    </Card>

                                    <Card variant="outlined">
                                        <CardContent>
                                            <Typography variant="h6" color="secondary">
                                                🦴 Переломи
                                            </Typography>
                                            <Typography variant="h3">
                                                {Math.round(result.fractureConfidence * 100)}%
                                            </Typography>
                                            <Chip
                                                label={result.hasFracture ? "ВИЯВЛЕНО" : "НОРМА"}
                                                color={result.hasFracture ? "error" : "success"}
                                                size="small"
                                            />
                                        </CardContent>
                                    </Card>
                                </Box>
                            </CardContent>
                        </Card>
                    )}
                </CardContent>
            </Card>
        </Box>
    );
};