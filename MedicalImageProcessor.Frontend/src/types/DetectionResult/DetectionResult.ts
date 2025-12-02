export interface DetectionResult {
  hasBrainTumor: boolean;
  brainTumorConfidence: number;
  hasFracture: boolean;
  fractureConfidence: number;
  imageId: string;
}