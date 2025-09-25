const BaseUrl = {
  // ImportBaseURL
  Homepage: '/',
  Login: '/login',
  Register: '/register',
  AdminLogin: '/adminlogin',

  //! Student Routes
  Student: '/student',
  StudentProfile: '/student/profile',
  ExamList: '/student/examlist',
  ExamResult: '/student/examresult',
  ExamResultDetail: '/student/examresult/:studentExamId',
  SendFeedback: '/student/feedback',
  //! Teacher Routes
  Lecturer: '/lecturer',
  Overview: '/lecturer/overview',
  SendFeedbackTeacher: '/lecturer/feedback',
  TeacherProfile: '/lecturer/profile',
  BankQuestion: '/lecturer/question-bank',
  AddQuestion: '/lecturer/question-bank/manage-question/add',
  ManageExam: '/lecturer/manage-exam',
  AddNewExamLecture: '/lecturer/manage-exam/add-new-exam',
  ExamSupervision: '/lecturer/exam-supervision',

  ExamResultTeacher: '/lecturer/examresult',
  ExamResultTeacherDetail: '/lecturer/examresult/:examId',

  GradeEssay: '/lecturer/examresult/grade-essay',

  TeacherExamResultDetail: '/lecturer/examresultdetail',
  ExamSupervisor: '/lecturer/exam-supervisor',
  TeacherManageRoom: '/lecturer/manage-room',
  //! Admin Routes
  AdminDashboard: '/admin',
  AdminProfile: '/admin/profile',
  AdminManageUsers: '/admin/manage-users',
  AdminBankQuestion: '/admin/question-bank',
  AdminAddQuestion: '/admin/question-bank/manage-question/add',
  AdminManageQuestion: '/admin/question-bank/manage-question',
  AdminAddNewUser: '/admin/manage-users/add-new-user',
  AdminAddNewRole: '/admin/manage-users/add-new-role',
  AdminManageSubject: '/admin/manage-subject',
  AdminManageClass: '/admin/manage-class',
  AdminManageRoom: '/admin/manage-room',
  AdminProhibited: '/admin/prohibited',
  AdminKeyboardShortcut: '/admin/keyboard-shortcut',
  AdminUserActivityLog: '/admin/user-activity-log',
  AdminSupervision: '/admin/exam-supervision',
  AdminManageExam: '/admin/manage-exam',
  AdminAddNewExam: '/admin/manage-exam/add-new-exam',
  AdminPermission: '/admin/permission',

  ExamResultAdmin: '/admin/examresult',
  ExamResultAdminDetail: '/admin/examresult/:examId',
  AdminExamSupervisor: '/admin/exam-supervisor',
  ViewFeedback: '/admin/feedback',
  //!SignalR
  SignalR: '/examHub',

  //! Supervisor Routes
  Supervisor: '/supervisor',
  SupervisorExamSupervision: '/supervisor/exam-supervision',
  SupervisorProfile: '/supervisor/profile',
  SupervisorExamSupervisor: '/supervisor/exam-supervisor',
  SupervisorFeedback: '/supervisor/feedback',
};

export default BaseUrl;
