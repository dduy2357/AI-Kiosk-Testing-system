import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

interface ExamProgressProps {
  progress: {
    startTime: string;
    duration: string;
    questionsCompleted: number;
    totalQuestions: number;
    timeElapsed: string;
    completionPercentage: number;
  };
}

export function ExamProgress({ progress }: Readonly<ExamProgressProps>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Tiến độ bài thi</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-600">Tiến độ hoàn thành</span>
            <span className="font-medium">{progress.completionPercentage}%</span>
          </div>

          <div className="h-2 w-full rounded-full bg-gray-200">
            <div
              className="h-2 rounded-full bg-blue-600 transition-all duration-300"
              style={{ width: `${progress.completionPercentage}%` }}
            />
          </div>

          <div className="grid grid-cols-4 gap-4 text-sm">
            <div>
              <span className="block text-gray-500">Bắt đầu</span>
              <span className="font-medium">{progress.startTime}</span>
            </div>
            <div>
              <span className="block text-gray-500">Thời lượng</span>
              <span className="font-medium">{progress.duration}</span>
            </div>
            <div>
              <span className="block text-gray-500">Đã trả lời</span>
              <span className="font-medium">{progress.questionsCompleted} câu</span>
            </div>
            <div>
              <span className="block text-gray-500">Còn lại</span>
              <span className="font-medium">{progress.timeElapsed}</span>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
