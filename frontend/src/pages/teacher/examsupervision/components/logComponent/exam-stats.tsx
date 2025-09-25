import { Card, CardContent } from '@/components/ui/card';
import { Clock, FileText, Target } from 'lucide-react';

interface ExamStatsProps {
  stats: {
    timeRemaining: number;
    questionsAnswered: number;
    totalQuestions: number;
    currentScore: number;
    maxScore: number;
    violations: number;
  };
}

export function ExamStats({ stats }: Readonly<ExamStatsProps>) {
  const statCards = [
    {
      title: 'Thời gian còn lại',
      value: `${stats.timeRemaining}m`,
      icon: Clock,
      color: 'text-blue-600',
      bgColor: 'bg-blue-50',
      borderColor: 'border-blue-200',
    },
    {
      title: 'Câu hỏi',
      value: `${stats.questionsAnswered}/${stats.totalQuestions}`,
      icon: FileText,
      color: 'text-green-600',
      bgColor: 'bg-green-50',
      borderColor: 'border-green-200',
    },
    {
      title: 'Điểm số',
      value: `${stats.currentScore}/${stats.maxScore}`,
      icon: Target,
      color: 'text-purple-600',
      bgColor: 'bg-purple-50',
      borderColor: 'border-purple-200',
    },
  ];

  return (
    <>
      {statCards.map((stat, index) => {
        return (
          <Card key={index} className={`${stat.bgColor} ${stat.borderColor} border-2`}>
            <CardContent className="p-4">
              <div className="flex items-center space-x-3">
                <div className={`h-8 w-3 ${stat.bgColor.replace('50', '500')} rounded`}></div>
                <div className="flex-1">
                  <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
                  <p className="text-sm text-gray-600">{stat.title}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        );
      })}
    </>
  );
}
