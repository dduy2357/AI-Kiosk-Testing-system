export interface Question {
  id: number;
  type: "multiple-choice" | "essay";
  difficulty: "easy" | "medium" | "hard";
  question: string;
  options?: string[];
  correctAnswer?: string;
  explanation?: string;
  tags: string[];
  views: number;
  accuracy: number;
  createdAt: string;
  updatedAt: string;
  usageCount: number;
}
