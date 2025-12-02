// pages/Results.tsx
import React from "react";
import { useResults } from "./useResults";
import { useNavigate } from "react-router-dom";
import "./results.css";

export const Results: React.FC = () => {
  const { results, loading, error } = useResults();
    const navigate = useNavigate();

    const goToDetection = () => {
        navigate("/detection");
    };

  if (loading) return <p>Loading results...</p>;
  if (error) return <p style={{ color: "red" }}>{error}</p>;
  if (results.length === 0) return <p>No results found.</p>;

  return (
    <div className="results-container">
      <div style={{ display: "inline-flex" , alignItems: "center", gap: 15 }}>
          <h2>Detection Results</h2>
          <button onClick={goToDetection} className="go-button">
          Go to Detection
        </button>
      </div>
      <table className="results-table">
        <thead>
          <tr>
            <th>Image ID</th>
            <th>Brain Tumor</th>
            <th>Brain Tumor Confidence</th>
            <th>Fracture</th>
            <th>Fracture Confidence</th>
          </tr>
        </thead>
        <tbody>
          {results.map((res) => (
            <tr key={res.imageId}>
              <td>{res.imageId}</td>
              <td>{res.hasBrainTumor ? "Yes" : "No"}</td>
              <td>{(res.brainTumorConfidence * 100).toFixed(2)}%</td>
              <td>{res.hasFracture ? "Yes" : "No"}</td>
              <td>{(res.fractureConfidence * 100).toFixed(2)}%</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
