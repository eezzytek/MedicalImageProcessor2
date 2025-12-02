export interface DetectionResult {
    imageId: string;
    hasBrainTumor: boolean;
    brainTumorConfidence: number;
    hasFracture: boolean;
    fractureConfidence: number;
    createdAt: string;
    image_url?: string;
}
