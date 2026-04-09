using Fusion;
using UnityEngine;
using Cinemachine; // Bắt buộc phải có dòng này để dùng Cinemachine

public class LocalPlayerCameraSetup : NetworkBehaviour
{
    public override void Spawned()
    {
        // Kiểm tra xem nhân vật này có phải của người chơi trên máy này không
        if (Object.HasInputAuthority)
        {
            // Tìm cái Virtual Camera trong scene (cái CM vcam1 bạn vừa tạo)
            CinemachineVirtualCamera vCam = FindObjectOfType<CinemachineVirtualCamera>();

            if (vCam != null)
            {
                // Gán chính nhân vật này (transform) làm mục tiêu để camera đi theo
                vCam.Follow = this.transform;
            }
            else
            {
                Debug.LogError("Không tìm thấy CinemachineVirtualCamera nào trong Scene! Hãy chắc chắn bạn đã tạo nó.");
            }
        }
    }
}