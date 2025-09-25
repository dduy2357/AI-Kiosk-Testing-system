import { Check } from 'lucide-react';

interface Step {
  id: number;
  title: string;
  description?: string;
  completed: boolean;
  current: boolean;
}

interface ProgressStepperProps {
  steps: Step[];
}

export function ProgressStepper({ steps }: Readonly<ProgressStepperProps>) {
  return (
    <div className="w-full border-b border-gray-200 bg-white px-6 py-4">
      <div className="flex items-center justify-center">
        <nav aria-label="Progress" className="flex items-center space-x-8">
          {steps.map((step, stepIdx) => (
            <div key={step.id} className="flex items-center">
              <div className="flex items-center space-x-3">
                <div className="flex items-center">
                  {step.completed ? (
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-600">
                      <Check className="h-4 w-4 text-white" />
                    </div>
                  ) : step.current ? (
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-600">
                      <span className="text-sm font-medium text-white">{step.id}</span>
                    </div>
                  ) : (
                    <div className="flex h-8 w-8 items-center justify-center rounded-full border-2 border-gray-300">
                      <span className="text-sm font-medium text-gray-500">{step.id}</span>
                    </div>
                  )}
                </div>
                <div className="flex flex-col">
                  <span
                    className={`text-sm font-medium ${
                      step.current || step.completed ? 'text-blue-600' : 'text-gray-500'
                    }`}
                  >
                    {step.title}
                  </span>
                  {step.description && (
                    <span className="text-xs text-gray-400">{step.description}</span>
                  )}
                </div>
              </div>
              {stepIdx < steps.length - 1 && (
                <div className="ml-8 h-0.5 w-16 bg-gray-200">
                  <div
                    className={`h-full transition-all duration-300 ${
                      step.completed ? 'w-full bg-blue-600' : 'w-0 bg-gray-200'
                    }`}
                  />
                </div>
              )}
            </div>
          ))}
        </nav>
      </div>
    </div>
  );
}
