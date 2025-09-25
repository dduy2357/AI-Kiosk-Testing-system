
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { t } from "i18next";
import { CheckCircle, AlertCircle, Lightbulb } from "lucide-react";
import { useTranslation } from "react-i18next";

interface Tip {
  title: string;
  description: string;
  icon: any;
  color: string;
}

const tips: Tip[] = [
  {
    title: t('AddQuestion.tip1'),
    description: t('AddQuestion.tip1Description'),
    icon: CheckCircle,
    color: "text-green-600",
  },
  {
    title: t('AddQuestion.tip2'),
    description: t('AddQuestion.tip2Description'),
    icon: AlertCircle,
    color: "text-blue-600",
  },
  {
    title: t('AddQuestion.tip3'),
    description: t('AddQuestion.tip3Description'),
    icon: Lightbulb,
    color: "text-orange-600",
  },
  {
    title: t('AddQuestion.tip4'),
    description: t('AddQuestion.tip4Description'),
    icon: AlertCircle,
    color: "text-red-600",
  },
];

export function TipsPanel() {
  const { t } = useTranslation('shared');
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center text-lg font-medium">
          <Lightbulb className="mr-2 h-5 w-5" />
          {t('AddQuestion.TipCreateQuestion')}
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {tips.map((tip, index) => {
          const Icon = tip.icon;
          return (
            <div key={index} className="flex items-start space-x-3">
              <Icon className={`mt-0.5 h-4 w-4 ${tip.color}`} />
              <div>
                <h4 className="text-sm font-medium text-gray-900">
                  {tip.title}
                </h4>
                <p className="mt-1 text-xs text-gray-500">{tip.description}</p>
              </div>
            </div>
          );
        })}

        <Separator />

        <div className="text-xs text-gray-500">
          <p className="mb-1 font-medium">
            {t('AddQuestion.Suggest')}
          </p>
          <p>{t('AddQuestion.SuggestDescription')}</p>
        </div>
      </CardContent>
    </Card>
  );
}
