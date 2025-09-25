import { useEffect, useState } from "react"
import { Loader2 } from "lucide-react"
import UserProfileComponent from "./components/user-profile"
import useGetDetailUser from "@/services/modules/user/hooks/useGetDetailUser"
import httpService from "@/services/httpService"

export default function UserProfilePage() {
    const [error, setError] = useState<string | null>(null)
    const user = httpService.getUserStorage()
    const { data: userDetail, isLoading, error: fetchError } = useGetDetailUser(user?.userID as string, {
        isTrigger: !!user?.userID,
    });

    useEffect(() => {
        if (fetchError) {
            setError("Không thể tải thông tin người dùng");
        } else if (userDetail) {
            setError(null); // Clear error when data is successfully loaded
        }
    }, [fetchError, userDetail]);

    if (!user?.userID) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <p>Không tìm thấy thông tin người dùng</p>
            </div>
        );
    }



    if (isLoading) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="flex items-center gap-2">
                    <Loader2 className="h-6 w-6 animate-spin" />
                    <span>Đang tải thông tin người dùng...</span>
                </div>
            </div>
        )
    }

    if (error) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="text-center">
                    <p className="text-red-600 mb-4">{error}</p>
                    <button
                        onClick={() => window.location.reload()}
                        className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                    >
                        Thử lại
                    </button>
                </div>
            </div>
        )
    }

    if (!userDetail) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <p>Không tìm thấy thông tin người dùng</p>
            </div>
        )
    }

    return <UserProfileComponent userProfile={userDetail} />
}
