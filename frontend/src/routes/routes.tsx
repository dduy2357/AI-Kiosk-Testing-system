import BaseUrl from '@/consts/baseUrl';
import { ROLE_ENUM } from '@/consts/role';
import withCheckRole from '@/HOCs/withCheckRole';
import ActivityLogDashboard from '@/pages/admin/activitylog';
import React, { Fragment, lazy } from 'react';

// Layouts
const DefaultLayout = lazy(() => import('@/layouts/DefaultLayout'));
const TeacherLayout = lazy(() => import('@/layouts/TeacherLayout'));
const AdminLayout = lazy(() => import('@/layouts/AdminLayout'));
const SupervisorLayout = lazy(() => import('@/layouts/SupervisorLayout'));

// Pages
const Profile = lazy(() => import('@/pages/user/index'));
const Login = lazy(() => import('@/pages/login'));
const Homepage = lazy(() => import('@/pages/homepage'));
const Register = lazy(() => import('@/pages/register'));
const ExamList = lazy(() => import('@/pages/examlist'));
const ExamResult = lazy(() => import('@/pages/examresult'));
const ExamResultDetail = lazy(() => import('@/pages/examresult/pages/exam-result-detail'));

const ExamDetail = lazy(() => import('@/pages/examlist/pages/ExamDetail'));
const Overview = lazy(() => import('@/pages/teacher/overview'));
const BankQuestion = lazy(() => import('@/pages/teacher/bankquestion'));
const QuestionBankDetailPage = lazy(
  () => import('@/pages/teacher/bankquestion/pages/bank-question-detail'),
);
const AddQuestion = lazy(() => import('@/pages/teacher/addquestion'));
const AdminLoginPage = lazy(() => import('@/pages/admin/adminlogin'));
const AdminManageUsers = lazy(() => import('@/pages/admin/manageuser'));
const AdminAddNewUser = lazy(() => import('@/pages/admin/manageuser/pages/AddNewUser'));
const AddNewRole = lazy(() => import('@/pages/admin/manageuser/pages/AddNewRole'));
const ManageSubject = lazy(() => import('@/pages/admin/managesubject'));
const ManageClass = lazy(() => import('@/pages/admin/manageclass'));
const ManageRoom = lazy(() => import('@/pages/admin/manageroom'));
const ProhibitedPage = lazy(() => import('@/pages/admin/prohibited'));
const KeyboardShortcut = lazy(() => import('@/pages/admin/keyboardshortcut'));
const StudentList = lazy(() => import('@/pages/admin/manageroom/pages/userinroom'));
const ManageExamLecture = lazy(() => import('@/pages/teacher/manageexam'));
const AddNewExamLecture = lazy(() => import('@/pages/teacher/manageexam/pages/AddNewExam'));
const ExamResultLecture = lazy(() => import('@/pages/teacher/examresult'));
const ExamResultLectureDetail = lazy(() => import('@/pages/teacher/examresultdetail'));
const ExamSupervision = lazy(() => import('@/pages/teacher/examsupervision/intex'));
const DetailExamSupervision = lazy(
  () => import('@/pages/teacher/examsupervision/pages/DetailExamSupervision'),
);
const Page404 = lazy(() => import('@/pages/Page404'));
const DetailConnectionSupervisor = lazy(
  () => import('@/pages/teacher/examsupervision/pages/DetailConnectionSupervisor'),
);
const GradeEssay = lazy(() => import('@/pages/teacher/examresultdetail/pages/gradeEssay/index'));
const ExamSupervisor = lazy(() => import('@/pages/teacher/examsupervisor'));

const ExamActivityLog = lazy(() => import('@/pages/teacher/examsupervision/pages/ExamActivityLog'));
const ExamActivityLogDetail = lazy(
  () => import('@/pages/teacher/examsupervision/pages/DetailLogExam'),
);
const SendFeedback = lazy(() => import('@/pages/feedback/index'));
const ViewFeedback = lazy(() => import('@/pages/admin/viewFeedback/index'));
const ViewViolation = lazy(() => import('@/pages/teacher/examsupervision/pages/Violation'));
const PermissionPage = lazy(() => import('@/pages/admin/permission/index'));

interface Route {
  name: string;
  path: string;
  isPrivateRoute?: boolean;
  layout: React.ComponentType<any>;
  routeChild: {
    name: string;
    path: string;
    component: React.ComponentType<any>;
    isPrivateRoute?: boolean;
  }[];
}

const routes: Route[] = [
  // Public routes
  {
    name: 'Login Layout',
    path: BaseUrl.Login,
    layout: Fragment,
    routeChild: [{ name: 'Login', path: BaseUrl.Login, component: Login }],
  },
  {
    name: 'Admin Login Layout',
    path: BaseUrl.AdminLogin,
    layout: Fragment,
    routeChild: [{ name: 'Admin Login', path: BaseUrl.AdminLogin, component: AdminLoginPage }],
  },
  {
    name: 'Register Layout',
    path: BaseUrl.Register,
    layout: Fragment,
    routeChild: [{ name: 'Register', path: BaseUrl.Register, component: Register }],
  },

  // Student routes
  {
    name: 'Home Layout',
    path: BaseUrl.Homepage,
    layout: withCheckRole(DefaultLayout, [ROLE_ENUM.Student]),
    isPrivateRoute: true,
    routeChild: [
      {
        name: 'Homepage',
        path: BaseUrl.Homepage,
        component: withCheckRole(Homepage, [ROLE_ENUM.Student]),
      },
    ],
  },
  {
    name: 'Student Layout',
    path: BaseUrl.Student,
    layout: withCheckRole(DefaultLayout, [ROLE_ENUM.Student]),
    isPrivateRoute: true,
    routeChild: [
      {
        name: 'Exam List',
        path: BaseUrl.ExamList,
        component: withCheckRole(ExamList, [ROLE_ENUM.Student]),
      },
      {
        name: 'Exam Result',
        path: BaseUrl.ExamResult,
        component: withCheckRole(ExamResult, [ROLE_ENUM.Student]),
      },
      {
        name: 'Exam Result Detail',
        path: BaseUrl.ExamResultDetail,
        component: withCheckRole(ExamResultDetail, [ROLE_ENUM.Student]),
      },
      {
        name: 'Feedback',
        path: BaseUrl.SendFeedback,
        component: withCheckRole(SendFeedback, [ROLE_ENUM.Student]),
      },
      {
        name: 'Student View Profile',
        path: `${BaseUrl.StudentProfile}`,
        component: withCheckRole(Profile, [ROLE_ENUM.Student]),
      },
    ],
  },
  {
    name: 'Exam Detail Layout',
    path: `${BaseUrl.ExamList}/:examId`,
    layout: withCheckRole(Fragment, [ROLE_ENUM.Student]),
    isPrivateRoute: true,
    routeChild: [
      {
        name: 'Exam Detail',
        path: `${BaseUrl.ExamList}/:examId`,
        component: withCheckRole(ExamDetail, [ROLE_ENUM.Student]),
      },
    ],
  },

  // Teacher routes
  {
    name: 'Teacher Layout',
    path: BaseUrl.Lecturer,
    layout: withCheckRole(TeacherLayout, [ROLE_ENUM.Lecture]),
    isPrivateRoute: true,
    routeChild: [
      {
        name: 'Overview',
        path: BaseUrl.Overview,
        component: withCheckRole(Overview, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'BankQuestion',
        path: BaseUrl.BankQuestion,
        component: withCheckRole(BankQuestion, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Bank Question Detail',
        path: `${BaseUrl.BankQuestion}/:questionBankId`,
        component: withCheckRole(QuestionBankDetailPage, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'AddNewQuestion',
        path: BaseUrl.AddQuestion,
        component: withCheckRole(AddQuestion, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Edit Question',
        path: `${BaseUrl.AddQuestion}/:questionId`,
        component: withCheckRole(AddQuestion, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Manage Exam Lecture',
        path: BaseUrl.ManageExam,
        component: withCheckRole(ManageExamLecture, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Add New Exam Lecture',
        path: BaseUrl.AddNewExamLecture,
        component: withCheckRole(AddNewExamLecture, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Edit Exam Lecture',
        path: `${BaseUrl.AddNewExamLecture}/:examId`,
        component: withCheckRole(AddNewExamLecture, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Exam Result Lecture',
        path: BaseUrl.ExamResultTeacher,
        component: withCheckRole(ExamResultLecture, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Exam Result Lecture Detail',
        path: BaseUrl.ExamResultTeacherDetail,
        component: withCheckRole(ExamResultLectureDetail, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Exam Supervision',
        path: BaseUrl.ExamSupervision,
        component: withCheckRole(ExamSupervision, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Exam Supervision Detail',
        path: `${BaseUrl.ExamSupervision}/:examId`,
        component: withCheckRole(DetailExamSupervision, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Detail Connection Supervisor',
        path: `${BaseUrl.ExamSupervision}/:examId/connection/:studentExamId`,
        component: withCheckRole(DetailConnectionSupervisor, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Exam Activity Log',
        path: `${BaseUrl.ExamSupervision}/:examId/examLog/:studentExamId`,
        component: withCheckRole(ExamActivityLog, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Exam Activity Log Detail',
        path: `${BaseUrl.ExamSupervision}/:examId/examLog/:studentExamId/:logId`,
        component: withCheckRole(ExamActivityLogDetail, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Grade Essay',
        path: `${BaseUrl.GradeEssay}/:examId/:studentExamId`,
        component: withCheckRole(GradeEssay, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Teacher Exam Result Detail',
        path: `${BaseUrl.TeacherExamResultDetail}/:studentExamId`,
        component: withCheckRole(ExamResultDetail, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Exam Supervisor',
        path: `${BaseUrl.ExamSupervisor}/:examId`,
        component: withCheckRole(ExamSupervisor, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Teacher View Violation',
        path: `${BaseUrl.ExamSupervisor}/:examId/violation/:studentExamId/`,
        component: withCheckRole(ViewViolation, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Feedback',
        path: BaseUrl.SendFeedbackTeacher,
        component: withCheckRole(SendFeedback, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Teacher View Profile',
        path: `${BaseUrl.TeacherProfile}`,
        component: withCheckRole(Profile, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Teacher Manage Room',
        path: BaseUrl.TeacherManageRoom,
        component: withCheckRole(ManageRoom, [ROLE_ENUM.Lecture]),
      },
      {
        name: 'Teacher Student List',
        path: `${BaseUrl.TeacherManageRoom}/:roomId`,
        component: withCheckRole(StudentList, [ROLE_ENUM.Lecture]),
      },
    ],
  },

  // Admin routes
  {
    name: 'Admin Layout',
    path: BaseUrl.AdminDashboard,
    layout: withCheckRole(AdminLayout, [ROLE_ENUM.ADMIN]),
    isPrivateRoute: true,
    routeChild: [
      {
        name: 'Admin Manage Users',
        path: BaseUrl.AdminManageUsers,
        component: withCheckRole(AdminManageUsers, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Add New User',
        path: BaseUrl.AdminAddNewUser,
        component: withCheckRole(AdminAddNewUser, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Edit User',
        path: `${BaseUrl.AdminAddNewUser}/:userId`,
        component: withCheckRole(AdminAddNewUser, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Add New Role',
        path: BaseUrl.AdminAddNewRole,
        component: withCheckRole(AddNewRole, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Edit Role',
        path: `${BaseUrl.AdminAddNewRole}/:roleId`,
        component: withCheckRole(AddNewRole, [ROLE_ENUM.ADMIN]),
      }, // Đổi tên để tránh nhầm lẫn
      {
        name: 'Admin Manage Subject',
        path: BaseUrl.AdminManageSubject,
        component: withCheckRole(ManageSubject, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Manage Class',
        path: BaseUrl.AdminManageClass,
        component: withCheckRole(ManageClass, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Manage Room',
        path: BaseUrl.AdminManageRoom,
        component: withCheckRole(ManageRoom, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Prohibited Page',
        path: BaseUrl.AdminProhibited,
        component: withCheckRole(ProhibitedPage, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Keyboard Shortcut',
        path: BaseUrl.AdminKeyboardShortcut,
        component: withCheckRole(KeyboardShortcut, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Student List',
        path: `${BaseUrl.AdminManageRoom}/:roomId`,
        component: withCheckRole(StudentList, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin User Activity Log',
        path: BaseUrl.AdminUserActivityLog,
        component: withCheckRole(ActivityLogDashboard, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Supervision',
        path: BaseUrl.AdminSupervision,
        component: withCheckRole(ExamSupervision, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Supervision Detail',
        path: `${BaseUrl.AdminSupervision}/:examId`,
        component: withCheckRole(DetailExamSupervision, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Detail Connection Supervisor',
        path: `${BaseUrl.AdminSupervision}/:examId/connection/:studentExamId`,
        component: withCheckRole(DetailConnectionSupervisor, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Exam Activity Log',
        path: `${BaseUrl.AdminSupervision}/:examId/examLog/:studentExamId`,
        component: withCheckRole(ExamActivityLog, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Exam Activity Log Detail',
        path: `${BaseUrl.AdminSupervision}/:examId/examLog/:studentExamId/:logId`,
        component: withCheckRole(ExamActivityLogDetail, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Manage Exam Admin',
        path: BaseUrl.AdminManageExam,
        component: withCheckRole(ManageExamLecture, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Exam Result Admin',
        path: BaseUrl.ExamResultAdmin,
        component: withCheckRole(ExamResultLecture, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Exam Result Admin Detail',
        path: BaseUrl.ExamResultAdminDetail,
        component: withCheckRole(ExamResultLectureDetail, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Add New Exam Admin',
        path: BaseUrl.AdminAddNewExam,
        component: withCheckRole(AddNewExamLecture, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Edit Exam Admin',
        path: `${BaseUrl.AdminAddNewExam}/:examId`,
        component: withCheckRole(AddNewExamLecture, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Exam Supervisor',
        path: `${BaseUrl.AdminExamSupervisor}/:examId`,
        component: withCheckRole(ExamSupervisor, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'View feedback',
        path: `${BaseUrl.ViewFeedback}`,
        component: withCheckRole(ViewFeedback, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin View Violation',
        path: `${BaseUrl.AdminSupervision}/:examId/violation/:studentExamId/`,
        component: withCheckRole(ViewViolation, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin View Profile',
        path: `${BaseUrl.AdminProfile}`,
        component: withCheckRole(Profile, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Permission Management',
        path: `${BaseUrl.AdminPermission}`,
        component: withCheckRole(PermissionPage, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin BankQuestion',
        path: BaseUrl.AdminBankQuestion,
        component: withCheckRole(BankQuestion, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Bank Question Detail',
        path: `${BaseUrl.AdminBankQuestion}/:questionBankId`,
        component: withCheckRole(QuestionBankDetailPage, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin AddNewQuestion',
        path: BaseUrl.AdminAddQuestion,
        component: withCheckRole(AddQuestion, [ROLE_ENUM.ADMIN]),
      },
      {
        name: 'Admin Edit Question',
        path: `${BaseUrl.AdminAddQuestion}/:questionId`,
        component: withCheckRole(AddQuestion, [ROLE_ENUM.ADMIN]),
      },
    ],
  },

  // Supervisor routes
  {
    name: 'Supervisor Layout',
    path: BaseUrl.Supervisor,
    layout: withCheckRole(SupervisorLayout, [ROLE_ENUM.SuperVisor]),
    isPrivateRoute: true,
    routeChild: [
      {
        name: 'Supervisor Exam Supervision',
        path: `${BaseUrl.SupervisorExamSupervision}`,
        component: withCheckRole(ExamSupervision, [ROLE_ENUM.SuperVisor]),
      },
      {
        name: 'Supervisor Exam Supervision Detail',
        path: `${BaseUrl.SupervisorExamSupervision}/:examId`,
        component: withCheckRole(DetailExamSupervision, [ROLE_ENUM.SuperVisor]),
      },
      {
        name: 'Detail Connection Supervisor',
        path: `${BaseUrl.SupervisorExamSupervision}/:examId/connection/:studentExamId`,
        component: withCheckRole(DetailConnectionSupervisor, [ROLE_ENUM.SuperVisor]),
      },
      {
        name: 'Supervisor Profile',
        path: `${BaseUrl.SupervisorProfile}`,
        component: withCheckRole(Profile, [ROLE_ENUM.SuperVisor]),
      },
      {
        name: 'Supervisor Exam Supervisor',
        path: `${BaseUrl.SupervisorExamSupervisor}/:examId`,
        component: withCheckRole(ExamSupervisor, [ROLE_ENUM.SuperVisor]),
      },
      {
        name: 'Supervisor Feedback',
        path: BaseUrl.SupervisorFeedback,
        component: withCheckRole(SendFeedback, [ROLE_ENUM.SuperVisor]),
      },
      {
        name: 'Supervisor Exam Activity Log',
        path: `${BaseUrl.SupervisorExamSupervision}/:examId/examLog/:studentExamId`,
        component: withCheckRole(ExamActivityLog, [ROLE_ENUM.SuperVisor]),
      },
      {
        name: 'Supervisor Exam Activity Log Detail',
        path: `${BaseUrl.SupervisorExamSupervision}/:examId/examLog/:studentExamId/:logId`,
        component: withCheckRole(ExamActivityLogDetail, [ROLE_ENUM.SuperVisor]),
      },
      {
        name: 'Supervisor View Violation',
        path: `${BaseUrl.SupervisorExamSupervision}/:examId/violation/:studentExamId/`,
        component: withCheckRole(ViewViolation, [ROLE_ENUM.SuperVisor]),
      },
    ],
  },

  // Fallback route
  {
    name: 'Not Found',
    path: '*',
    layout: Fragment,
    routeChild: [{ name: 'Page404', path: '*', component: Page404 }],
  },
];

export default routes;
