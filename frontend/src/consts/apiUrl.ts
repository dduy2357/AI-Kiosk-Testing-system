const ROOT_URL = import.meta.env.VITE_BASE_URL;
export const AI_URL = import.meta.env.VITE_AI_API_URL;

// Dont remove this command
//!User
export const USER_URL = `${ROOT_URL}/api/User`;

//!Authorize
export const AUTHORIZE_URL = `${ROOT_URL}/api/Authorize`;

//!Major
export const MAJOR_URL = `${ROOT_URL}/api/Majors`;

//!Campus
export const CAMPUS_URL = `${ROOT_URL}/api/Campus`;

//!Department
export const DEPARTMENT_URL = `${ROOT_URL}/api/Departments`;

//!Position
export const POSITION_URL = `${ROOT_URL}/api/Positions`;

//!Specialization
export const SPECIALIZATION_URL = `${ROOT_URL}/api/Specializations`;

export const AUTH_URL = `${ROOT_URL}/api/auth`;
//!Subject
export const SUBJECT_URL = `${ROOT_URL}/api/Subject`;

//!Class
export const CLASS_URL = `${ROOT_URL}/api/Classes`;

//!Room
export const ROOM_URL = `${ROOT_URL}/api/Room`;

//!Question
export const QUESTION_URL = `${ROOT_URL}/api/Questions`;

//!Question Bank
export const QUESTION_BANK_URL = `${ROOT_URL}/api/QuestionBank`;

//!Prohibited
export const PROHIBITED_URL = `${ROOT_URL}/api/ProhibitedApp`;

//!KeyboardShortcut
export const KEYBOARD_SHORTCUT_URL = `${ROOT_URL}/api/DisabledKey`;

//!User in Room
export const USER_IN_ROOM_URL = `${ROOT_URL}/api/RoomUser`;

//!Google Login
export const LOGIN_GOOGLE = `${AUTH_URL}/google-callback1`;
export const GEET_LINK_GOOGLE = `${AUTH_URL}/get-link-google`;

//!Student Exam
export const STUDENT_EXAM_URL = `${ROOT_URL}/api/StudentExam`;

//!Teacher Exam
export const TEACHER_EXAM_URL = `${ROOT_URL}/api/Exam`;

//!Log
export const LOG_URL = `${ROOT_URL}/api/Log`;

//!Monitor
export const MONITOR_URL = `${ROOT_URL}/api/Monitor`;

//!Face Capture
export const FACE_CAPTURE_URL = `${ROOT_URL}/api/FaceCapture`;

//! User Activity Log
export const ACTIVITY_LOG_URL = `${ROOT_URL}/api/Log`;

//!SignalR
export const SIGNALR_URL = `${ROOT_URL}/examHub`;
export const NOTIFY_HUB = `${ROOT_URL}/notifyHub`;

//!Alert
export const NOTIFICATION_URL = `${ROOT_URL}/api/Notification`;

//!Exam Supervisor
export const EXAM_SUPERVISOR_URL = `${ROOT_URL}/api/ExamSupervisor`;

//! Feedback
export const FEEDBACK_URL = `${ROOT_URL}/api/Feedback`;

//! Violation
export const VIOLATION_URL = `${ROOT_URL}/api/StudentViolation`;

//! Violation
export const AMAZON_URL = `${ROOT_URL}/api/AmazonS3`;
