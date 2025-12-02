import React from "react";
import { useResults } from "./useResults.ts";
import { useNavigate } from "react-router-dom";
import {
    Box, Typography, Table, TableBody, TableCell, TableContainer,
    TableHead, TableRow, Paper, Button, CircularProgress, Alert, Chip, Avatar
} from "@mui/material";
import ArrowForwardIcon from "@mui/icons-material/ArrowForward";

export const Results: React.FC = () => {
    const { results, loading, error } = useResults();
    const navigate = useNavigate();

    if (loading) {
        return (
            <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
                <CircularProgress size={60} />
            </Box>
        );
    }

    if (error) {
        return <Alert severity="error" sx={{ m: 4 }}>{error}</Alert>;
    }

    return (
        <Box sx={{ maxWidth: 1400, mx: "auto", p: 3 }}>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={4}>
                <Typography variant="h4" fontWeight="bold" color="primary">
                    Історія аналізів
                </Typography>
                <Button
                    variant="contained"
                    size="large"
                    endIcon={<ArrowForwardIcon />}
                    onClick={() => navigate("/detection")}
                >
                    Новий аналіз
                </Button>
            </Box>

            {results.length === 0 ? (
                <Alert severity="info" sx={{ fontSize: "1.2rem" }}>
                    У вас ще немає аналізів. Почніть перший!
                </Alert>
            ) : (
                <TableContainer component={Paper} elevation={8}>
                    <Table>
                        <TableHead>
                            <TableRow sx={{ backgroundColor: "#1976d2" }}>
                                <TableCell sx={{ color: "white", fontWeight: "bold" }}>Фото</TableCell>
                                <TableCell sx={{ color: "white", fontWeight: "bold" }}>Пухлина</TableCell>
                                <TableCell sx={{ color: "white", fontWeight: "bold" }}>Впевненість</TableCell>
                                <TableCell sx={{ color: "white", fontWeight: "bold" }}>Перелом</TableCell>
                                <TableCell sx={{ color: "white", fontWeight: "bold" }}>Впевненість</TableCell>
                                <TableCell sx={{ color: "white", fontWeight: "bold" }}>Дата</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {results.map((res) => {
                                // Витягуємо URL зображення з imageId (Supabase повертає повний URL)
                                const imageUrl = res.image_url
                                    ? res.image_url
                                    : `https://bixxkxjdyuapgnbzewdo.supabase.co/storage/v1/object/public/medical-images/${res.imageId}.jpg`;
                                return (
                                    <TableRow key={res.imageId} hover sx={{ '&:hover': { backgroundColor: "#f5f7fa" } }}>
                                        <TableCell>
                                            <Avatar
                                                src={imageUrl}
                                                alt="Medical image"
                                                variant="rounded"
                                                sx={{ width: 80, height: 80, objectFit: "cover" }}
                                            >
                                                IMG
                                            </Avatar>
                                        </TableCell>
                                        <TableCell>
                                            <Chip
                                                label={res.hasBrainTumor ? "Виявлено" : "Ні"}
                                                color={res.hasBrainTumor ? "error" : "success"}
                                                size="small"
                                            />
                                        </TableCell>
                                        <TableCell sx={{ fontWeight: "bold" }}>
                                            {(res.brainTumorConfidence * 100).toFixed(1)}%
                                        </TableCell>
                                        <TableCell>
                                            <Chip
                                                label={res.hasFracture ? "Виявлено" : "Ні"}
                                                color={res.hasFracture ? "error" : "success"}
                                                size="small"
                                            />
                                        </TableCell>
                                        <TableCell sx={{ fontWeight: "bold" }}>
                                            {(res.fractureConfidence * 100).toFixed(1)}%
                                        </TableCell>
                                        <TableCell>
                                            {new Date(res.createdAt).toLocaleString("uk-UA")}
                                        </TableCell>
                                    </TableRow>
                                );
                            })}
                        </TableBody>
                    </Table>
                </TableContainer>
            )}
        </Box>
    );
};
