import { useState, useEffect } from "react";
import { useAuth } from "../../contexts/AuthContext";

export interface DetectionResult {
    imageId: string;
    hasBrainTumor: boolean;
    brainTumorConfidence: number;
    hasFracture: boolean;
    fractureConfidence: number;
    createdAt: string;
    image_url?: string;
}

export function useResults() {
    const { user } = useAuth();
    const [results, setResults] = useState<DetectionResult[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!user?.token) {
            setLoading(false);
            return;
        }

        const fetchResults = async () => {
            setLoading(true);
            setError(null);

            try {
                const response = await fetch(`${import.meta.env.VITE_API_URL}/api/results`, {
                    headers: {
                        Authorization: `Bearer ${user.token}`,
                    },
                });

                if (!response.ok) {
                    const err = await response.json();
                    throw new Error(err.error || "Не вдалося завантажити історію");
                }

                const data = await response.json();
                setResults(data);
            } catch (err: any) {
                console.error("Results error:", err);
                setError(err.message);

                // Мок для розробки (видалити коли все працює)
                setResults([
                    { imageId: "demo-001", hasBrainTumor: true, brainTumorConfidence: 0.92, hasFracture: false, fractureConfidence: 0.05, createdAt: new Date().toISOString() },
                    { imageId: "demo-002", hasBrainTumor: false, brainTumorConfidence: 0.12, hasFracture: true, fractureConfidence: 0.78, createdAt: new Date().toISOString() },
                ]);
            } finally {
                setLoading(false);
            }
        };

        fetchResults();
    }, [user]);

    return { results, loading, error };
}
