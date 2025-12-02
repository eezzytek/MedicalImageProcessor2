// hooks/useResults.ts
import { useState, useEffect } from "react";
import { useAuth } from "../../contexts/AuthContext";
import type { DetectionResult } from "../../types/DetectionResult/DetectionResult";

export function useResults() {
  const { user } = useAuth();
  const [results, setResults] = useState<DetectionResult[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!user) return; // don't fetch if not logged in

    const fetchResults = async () => {
      setLoading(true);
      setError(null);

      try {

        // IMPORTANT FIX: YOUR BACK MUST TAKE ID FROM TOKEN ADD TOKEN PARSE TO THE BACK + ADD ID TO THE CLAIMS
        const token = user.token;
        const response = await fetch(`${import.meta.env.VITE_API_URL}/api/results`, {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData?.message || "Failed to fetch results");
        }

        const data: DetectionResult[] = await response.json();
        setResults(data);
      } catch (err: any) {
        const mockData: DetectionResult[] = [
          {
            hasBrainTumor: true,
            brainTumorConfidence: 0.92,
            hasFracture: false,
            fractureConfidence: 0,
            imageId: "IMG001",
          },
          {
            hasBrainTumor: false,
            brainTumorConfidence: 0,
            hasFracture: true,
            fractureConfidence: 0.78,
            imageId: "IMG002",
          },
          {
            hasBrainTumor: false,
            brainTumorConfidence: 0,
            hasFracture: false,
            fractureConfidence: 0,
            imageId: "IMG003",
          },
        ];

        setResults(mockData);
        // UNCOMMENT IT WHEN /api/results is ready  
        // setError(err.message || "Something went wrong");
      } finally {
        setLoading(false);
      }
    };

    fetchResults();
  }, [user]);

  return { results, loading, error };
}
