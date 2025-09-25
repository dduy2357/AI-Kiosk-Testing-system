import { Card, CardContent } from '@/components/ui/card';
import { Users, Target, TrendingUp } from 'lucide-react';

interface Stats {
  totalStudents: number;
  averageScore: number;
  passRate: number;
}

interface ExamOverviewProps {
  stats: Stats;
}

export function ExamOverview({ stats }: Readonly<ExamOverviewProps>) {
  const statCards = [
    {
      title: 'Tổng số học sinh',
      value: stats.totalStudents.toString(),
      icon: Users,
      bgColor: 'bg-blue-50',
      iconColor: 'text-blue-600',
    },
    {
      title: 'Điểm trung bình',
      value: stats.averageScore.toString(),
      icon: Target,
      bgColor: 'bg-green-50',
      iconColor: 'text-green-600',
    },
    {
      title: 'Tỷ lệ đạt',
      value: `${stats.passRate}%`,
      icon: TrendingUp,
      bgColor: 'bg-purple-50',
      iconColor: 'text-purple-600',
    },
    // {
    //   title: "Thời gian TB",
    //   value: `${stats.averageTime} phút`,
    //   icon: Clock,
    //   bgColor: "bg-orange-50",
    //   iconColor: "text-orange-600",
    // },
  ];

  return (
    <div className="grid grid-cols-3 gap-6">
      {statCards.map((stat, index) => {
        const Icon = stat.icon;
        return (
          <Card key={index}>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="mb-1 text-sm text-gray-600">{stat.title}</p>
                  <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
                </div>
                <div
                  className={`h-12 w-12 ${stat.bgColor} flex items-center justify-center rounded-lg`}
                >
                  <Icon className={`h-6 w-6 ${stat.iconColor}`} />
                </div>
              </div>
            </CardContent>
          </Card>
        );
      })}
    </div>
  );
}
