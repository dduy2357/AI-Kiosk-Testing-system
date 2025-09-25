import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Plus,
  Search,
  Filter,
  Edit,
  FileText,
  BookOpen,
  Star,
  CheckCircle,
  Clock,
  User,
} from 'lucide-react';
import { Input } from '@/components/ui/input';
import {
  Question,
  QuestionBankDetail,
} from '@/services/modules/bankquestion/interfaces/bankquestion.interface';
import { useNavigate } from 'react-router-dom';
import BaseUrl from '@/consts/baseUrl';
import { useState } from 'react';
import httpService from '@/services/httpService';
import { QuestionList } from '@/services/modules/question/interfaces/question.interface';
import { Switch } from '@/components/ui/switch';
import questionService from '@/services/modules/question/question.service';
import { showSuccess } from '@/helpers/toast';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useTranslation } from 'react-i18next';

interface QuestionsListProps {
  questions: Question[] | [];
  questionbankDetail: QuestionBankDetail | null;
  refetch: () => void;
}

export function QuestionsList({
  questions,
  questionbankDetail,
  refetch,
}: Readonly<QuestionsListProps>) {
  const navigate = useNavigate();
  const { t } = useTranslation('shared');
  const [currentPage, setCurrentPage] = useState(1);
  const [filterType, setFilterType] = useState<string>('all');
  const [searchQuery, setSearchQuery] = useState<string>(''); // Thêm state cho search
  const itemsPerPage = 5;
  const roleId = httpService.getUserStorage()?.roleId.at(0);

  const getDifficultyColor = (level: number) => {
    switch (level) {
      case 1:
        return 'bg-green-100 text-green-800 border-green-200';
      case 2:
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 3:
        return 'bg-red-100 text-red-800 border-red-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  const getDifficultyText = (level: number) => {
    switch (level) {
      case 1:
        return t('QuestionCard.difficultyEasy');
      case 2:
        return t('QuestionCard.difficultyMedium');
      case 3:
        return t('QuestionCard.difficultyHard');
      default:
        return t('QuestionCard.difficultyUndefined');
    }
  };

  const getStatusColor = (status: number) => {
    switch (status) {
      case 1:
        return 'bg-green-100 text-green-800 border-green-200';
      case 0:
        return 'bg-gray-100 text-gray-800 border-gray-200';
      default:
        return 'bg-blue-100 text-blue-800 border-blue-200';
    }
  };

  const getStatusText = (status: number) => {
    switch (status) {
      case 1:
        return t('QuestionCard.statusActive');
      case 0:
        return t('QuestionCard.statusInactive');
      default:
        return t('QuestionCard.statusDraft');
    }
  };

  const getQuestionTypeText = (type: number) => {
    switch (type) {
      case 1:
        return t('QuestionCard.questionTypeMultipleChoice');
      case 2:
        return t('QuestionCard.questionTypeEssay');
      case 3:
        return t('QuestionCard.questionTypeTrueFalse');
      default:
        return t('QuestionCard.questionTypeOther');
    }
  };

  // Lọc câu hỏi dựa trên search query và type
  const filteredQuestions =
    questions?.filter((question) => {
      const matchesType = filterType === 'all' || question.type === parseInt(filterType);
      const matchesSearch = searchQuery
        ? question.content.toLowerCase().includes(searchQuery.toLowerCase())
        : true;
      return matchesType && matchesSearch;
    }) ?? [];

  // Calculate paginated questions
  const totalPages = Math.ceil((filteredQuestions?.length ?? 0) / itemsPerPage);
  const paginatedQuestions =
    filteredQuestions?.slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage) ?? [];

  // Reset to page 1 when filter or search changes
  const handleFilterChange = (value: string) => {
    setFilterType(value);
    setCurrentPage(1);
  };

  // Xử lý thay đổi search input
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchQuery(e.target.value);
    setCurrentPage(1);
  };

  const handlePageChange = (page: number) => {
    if (page > 0 && page <= totalPages) {
      setCurrentPage(page);
    }
  };

  const handleEditQuestion = (newquestion: Question) => {
    const question: QuestionList = {
      ...newquestion,
      questionBankId: questionbankDetail?.questionBankId ?? '',
      questionBankName: questionbankDetail?.questionBankName ?? '',
      subjectName: questionbankDetail?.subjectName ?? '',
    };
    roleId === 2
      ? navigate(`${BaseUrl.AddQuestion}`, { state: { question } })
      : navigate(`${BaseUrl.AdminAddQuestion}`, { state: { question } });
  };

  const handleAddQuestion = () => {
    const question = {
      questionBankId: questionbankDetail?.questionBankId ?? '',
      questionBankName: questionbankDetail?.questionBankName ?? '',
      subjectName: questionbankDetail?.subjectName ?? '',
      questionId: '',
      subjectId: questionbankDetail?.subjectId ?? '',
      content: '',
      type: 'multiple-choice',
      difficultLevel: 1,
      point: 1,
      options: [],
      correctAnswer: '',
      explanation: '',
      objectFile: '',
      status: 1,
      creatorId: httpService.getUserStorage()?.userID ?? '',
    };
    roleId === 4
      ? navigate(`${BaseUrl.AdminAddQuestion}`, { state: { question } })
      : navigate(`${BaseUrl.AddQuestion}`, { state: { question } });
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>
            {t('BankQuestion.QuestionList')} ({filteredQuestions?.length})
          </CardTitle>
          <Button onClick={handleAddQuestion}>
            <Plus className="mr-2 h-4 w-4" />
            {t('BankQuestion.AddQuestion')}
          </Button>
        </div>
        <div className="mt-4 flex items-center space-x-4">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 transform text-gray-400" />
            <Input
              placeholder={t('BankQuestion.SearchQuestion')}
              className="pl-10"
              value={searchQuery}
              onChange={handleSearchChange}
            />
          </div>
          <Select onValueChange={handleFilterChange} value={filterType}>
            <SelectTrigger className="w-[180px]">
              <Filter className="mr-2 h-4 w-4" />
              <SelectValue placeholder="Loại câu hỏi" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">{t('BankQuestion.All')}</SelectItem>
              <SelectItem value="1">{t('BankQuestion.MultipleChoice')}</SelectItem>
              <SelectItem value="0">{t('BankQuestion.Essay')}</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </CardHeader>
      <CardContent>
        {filteredQuestions.length === 0 ? (
          <div className="py-12 text-center">
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-100">
              <FileText className="h-8 w-8 text-gray-400" />
            </div>
            <h3 className="mb-2 text-lg font-medium text-gray-900">
              {t('BankQuestion.NoQuestions')}
            </h3>
            <p className="mb-4 text-gray-500">{t('BankQuestion.StartCreatingQuestion')}</p>
          </div>
        ) : (
          <>
            <div className="space-y-4">
              {paginatedQuestions.map((question) => (
                <Card
                  key={question?.questionId}
                  className="w-full transition-shadow duration-200 hover:shadow-lg"
                >
                  <CardHeader className="pb-3">
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <CardTitle className="mb-2 line-clamp-2 text-lg font-semibold">
                          {question.content}
                        </CardTitle>
                        <div className="mb-3 flex flex-wrap gap-2">
                          <Badge
                            variant="outline"
                            className={getDifficultyColor(question.difficultLevel)}
                          >
                            <Star className="mr-1 h-3 w-3" />
                            {getDifficultyText(question.difficultLevel)}
                          </Badge>
                          <Badge variant="outline" className={getStatusColor(question.status)}>
                            <CheckCircle className="mr-1 h-3 w-3" />
                            {getStatusText(question.status)}
                          </Badge>
                          <Badge
                            variant="outline"
                            className="border-blue-200 bg-blue-100 text-blue-800"
                          >
                            {getQuestionTypeText(question.type)}
                          </Badge>
                        </div>
                      </div>
                      <div className="ml-2 flex gap-1">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleEditQuestion(question)}
                          className="h-8 w-8 p-0"
                        >
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Switch
                          checked={question.status === 1}
                          onCheckedChange={async () => {
                            await questionService.toggleQuestion(question?.questionId);
                            showSuccess(t('QuestionCard.ToggleSuccess'));
                            refetch();
                          }}
                          className="mt-1.5 h-[20px] w-[36px] data-[state=checked]:bg-primary data-[state=unchecked]:bg-input [&>span]:h-4 [&>span]:w-4 [&>span]:data-[state=checked]:translate-x-4 [&>span]:data-[state=unchecked]:translate-x-0"
                          aria-label="Toggle question status"
                        />
                      </div>
                    </div>
                  </CardHeader>
                  <CardContent className="pt-0">
                    <div className="space-y-4">
                      {/* Image Preview */}
                      {question.objectFile && (
                        <div className="space-y-2">
                          <span className="text-sm font-medium">
                            {t('QuestionCard.imageLabel')}
                          </span>
                          <img
                            src={question.objectFile}
                            alt="Question preview"
                            className="max-h-24 w-auto rounded-md border border-gray-200 object-contain"
                            onError={(e) => {
                              e.currentTarget.style.display = 'none'; // Hide image if it fails to load
                            }}
                          />
                        </div>
                      )}
                      {/* Subject and Question Bank Info */}
                      <div className="grid grid-cols-1 gap-3 text-sm md:grid-cols-2">
                        <div className="flex items-center gap-2">
                          <BookOpen className="h-4 w-4 text-blue-500" />
                          <span className="font-medium">{t('QuestionCard.subjectLabel')}</span>
                          <span className="text-gray-600">{questionbankDetail?.subjectName}</span>
                        </div>
                        <div className="flex items-center gap-2">
                          <FileText className="h-4 w-4 text-green-500" />
                          <span className="font-medium">{t('QuestionCard.questionBankLabel')}</span>
                          <span className="text-gray-600">
                            {questionbankDetail?.questionBankName}
                          </span>
                        </div>
                      </div>
                      {/* Points */}
                      <div className="flex items-center gap-2 text-sm">
                        <div className="flex items-center gap-1">
                          <span className="font-medium">{t('QuestionCard.pointsLabel')}</span>
                          <Badge variant="secondary" className="bg-purple-100 text-purple-800">
                            {question.point} {t('QuestionCard.points')}
                          </Badge>
                        </div>
                      </div>
                      {/* Options */}
                      {question.options && question.options.length > 0 && (
                        <div className="space-y-2">
                          <span className="text-sm font-medium">
                            {t('QuestionCard.optionsLabel')}
                          </span>
                          <div className="space-y-1">
                            {question.options.map((option, index) => (
                              <div
                                key={index}
                                className={`rounded-md border p-2 text-sm ${
                                  option === question.correctAnswer
                                    ? 'border-green-200 bg-green-50 text-green-800'
                                    : 'border-gray-200 bg-gray-50'
                                }`}
                              >
                                <span className="mr-2 font-medium">
                                  {String.fromCharCode(65 + index)}.
                                </span>
                                {option}
                                {option === question.correctAnswer && (
                                  <CheckCircle className="ml-2 inline h-4 w-4 text-green-600" />
                                )}
                              </div>
                            ))}
                          </div>
                        </div>
                      )}
                      {/* Explanation */}
                      {question.explanation && (
                        <div className="space-y-2">
                          <span className="text-sm font-medium">
                            {t('QuestionCard.explanationLabel')}
                          </span>
                          <p className="rounded-md border border-blue-200 bg-blue-50 p-3 text-sm text-gray-600">
                            {question.explanation}
                          </p>
                        </div>
                      )}
                      {/* Footer Info */}
                      <div className="flex items-center justify-between border-t border-gray-100 pt-3 text-xs text-gray-500">
                        <div className="flex items-center gap-2">
                          <User className="h-3 w-3" />
                          <span>ID: {question.creatorId}</span>
                        </div>
                        <div className="flex items-center gap-2">
                          <Clock className="h-3 w-3" />
                          <span>ID câu hỏi: {question.questionId.slice(0, 8)}...</span>
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
            {totalPages > 1 && (
              <div className="mt-4 flex justify-center gap-2">
                <Button
                  variant="outline"
                  onClick={() => handlePageChange(currentPage - 1)}
                  disabled={currentPage === 1}
                >
                  Previous
                </Button>
                {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
                  <Button
                    key={page}
                    variant={currentPage === page ? 'default' : 'outline'}
                    onClick={() => handlePageChange(page)}
                  >
                    {page}
                  </Button>
                ))}
                <Button
                  variant="outline"
                  onClick={() => handlePageChange(currentPage + 1)}
                  disabled={currentPage === totalPages}
                >
                  Next
                </Button>
              </div>
            )}
          </>
        )}
      </CardContent>
    </Card>
  );
}
