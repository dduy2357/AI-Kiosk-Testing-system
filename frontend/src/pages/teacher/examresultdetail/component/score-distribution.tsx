import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

interface ScoreRange {
  range: string;
  count: number;
  percentage: number;
  color: string;
}

interface ScoreDistributionProps {
  data: ScoreRange[];
}

export function ScoreDistribution({ data }: Readonly<ScoreDistributionProps>) {
  const maxCount = Math.max(...data.map((item) => item.count));

  return (
    <Card>
      <CardHeader>
        <CardTitle>Phân bố điểm số</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {data.map((item, index) => (
            <div key={index} className="flex items-center justify-between">
              <div className="flex w-20 items-center space-x-3">
                <span className="text-sm font-medium text-gray-700">{item.range}</span>
              </div>
              <div className="mx-4 flex-1">
                <div className="h-2 w-full rounded-full bg-gray-200">
                  <div
                    className={`h-2 rounded-full ${item.color}`}
                    style={{ width: `${(item.count / maxCount) * 100}%` }}
                  />
                </div>
              </div>
              <div className="w-16 text-right">
                <div className="text-lg font-bold text-gray-900">{item.count}</div>
                <div className="text-xs text-gray-500">{item.percentage}%</div>
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
