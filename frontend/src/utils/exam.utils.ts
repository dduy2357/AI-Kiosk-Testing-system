export const parseOptions = (optionsString: string): string[] => {
  try {
    return JSON.parse(optionsString);
  } catch {
    return [];
  }
};

export const getOptionLabel = (index: number): string => {
  return String.fromCharCode(65 + index); // A, B, C, D...
};

export const findOptionIndex = (options: string[], answer: string): number => {
  return options.findIndex((option) => option === answer);
};

export const getScoreColor = (score: number, total = 10): string => {
  const percentage = (score / total) * 100;
  if (percentage >= 80) return 'text-green-600';
  if (percentage >= 60) return 'text-yellow-600';
  return 'text-red-600';
};

export const formatDuration = (minutes: number): string => {
  if (minutes < 60) {
    return `${minutes} phút`;
  }
  const hours = Math.floor(minutes / 60);
  const remainingMinutes = minutes % 60;
  return `${hours}h ${remainingMinutes}m`;
};

export const formatDurationSecond = (seconds: number) => {
  if (seconds < 60) {
    return `${seconds}s`;
  }
  if (seconds < 3600) {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}m ${remainingSeconds}s`;
  }
  const hours = Math.floor(seconds / 3600);
  const remainingMinutes = Math.floor((seconds % 3600) / 60);
  const remainingSeconds = seconds % 60;
  return `${hours}h ${remainingMinutes}m ${remainingSeconds}s`;
};

export const getStudentInitials = (name: string): string => {
  if (!name) return 'HS';
  const words = name.split(' ');
  if (words.length >= 2) {
    return (words[0][0] + words[words.length - 1][0]).toUpperCase();
  }
  return name.substring(0, 2).toUpperCase();
};
