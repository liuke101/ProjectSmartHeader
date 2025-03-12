using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExplosionCamera : MonoBehaviour
{
    [Header("相机设置")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera pipCamera;
    [SerializeField] private RenderTexture pipRenderTexture;
    [SerializeField] private RawImage pipDisplay;

    [Header("爆炸视角设置")]
    [SerializeField] private float explosionDuration = 3f;
    [SerializeField] private Vector2 pipDisplaySize = new Vector2(647.12f, 367.18f);
    [SerializeField] private float explosionViewHeight = 50f; // 爆炸点上方高度
    [SerializeField] private bool smoothTransition = true; // 是否平滑过渡
    [SerializeField] private float transitionSpeed = 5f; // 平滑过渡速度

    // 状态变量
    private bool isShowingExplosion = false;
    private float explosionTimer = 0f;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isTransitioning = false;
    private Coroutine returnCoroutine = null;
    private Transform cameraParent;
    private Vector3 localPos;
    private Quaternion localRot;
    
    // 缓存主相机的原始控制脚本
    private CameraController cameraControl;

    private void Awake()
    {
        // 初始化时隐藏PIP显示
        if (pipDisplay != null)
        {
            pipDisplay.gameObject.SetActive(false);
        }
        
        // 确保PIP相机设置正确
        if (pipCamera != null)
        {
            pipCamera.targetTexture = pipRenderTexture;
        }
        
        // 获取相机控制脚本
        if (mainCamera != null)
        {
            cameraControl = mainCamera.GetComponent<CameraController>();
        }
    }

    private void Start()
    {
        // 设置PIP显示尺寸
        if (pipDisplay != null)
        {
            pipDisplay.rectTransform.sizeDelta = pipDisplaySize;
        }
    }

    private void Update()
    {
        if (isShowingExplosion)
        {
            // 处理平滑过渡
            if (smoothTransition && isTransitioning)
            {
                mainCamera.transform.position = Vector3.Lerp(
                    mainCamera.transform.position, 
                    targetPosition, 
                    Time.deltaTime * transitionSpeed);
                
                mainCamera.transform.rotation = Quaternion.Slerp(
                    mainCamera.transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * transitionSpeed);
                
                // 如果接近目标位置和旋转，结束过渡
                if (Vector3.Distance(mainCamera.transform.position, targetPosition) < 0.1f &&
                    Quaternion.Angle(mainCamera.transform.rotation, targetRotation) < 1f)
                {
                    isTransitioning = false;
                }
            }

            // 爆炸视角倒计时
            explosionTimer -= Time.deltaTime;
            if (explosionTimer <= 0)
            {
                EndExplosionView();
            }
        }
        else
        {
            // 非爆炸视角时，让PIP相机跟随主相机
            if (pipCamera != null && mainCamera != null)
            {
                pipCamera.transform.position = mainCamera.transform.position;
                pipCamera.transform.rotation = mainCamera.transform.rotation;
            }
        }
    }
    public void TriggerExplosion(Vector3 explosionPosition)
    {
        // 取消之前的协程(如果存在)
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        // 保存原始状态
        if (!isShowingExplosion)
        {
            // 保存世界坐标和旋转
            originalPosition = mainCamera.transform.position;
            originalRotation = mainCamera.transform.rotation;
        }

        // 禁用相机控制脚本
        if (cameraControl != null && cameraControl.enabled)
        {
            cameraControl.enabled = false;
        }

        // 计算爆炸位置正上方的目标世界位置
        targetPosition = new Vector3(
            explosionPosition.x, 
            explosionPosition.y + explosionViewHeight, 
            explosionPosition.z);
    
        // 面向地面的旋转
        targetRotation = Quaternion.Euler(90f, 0f, 0f);
    
        // Debug.Log($"爆炸位置: {explosionPosition}, 目标世界位置: {targetPosition}");
    
        // 关键修复：如果相机有父对象，需要处理世界坐标转换
        Transform parent = mainCamera.transform.parent;
        if (parent != null)
        {
            // 将目标世界坐标转换为父对象的本地坐标系
            Vector3 localTarget = parent.InverseTransformPoint(targetPosition);
            localTarget.y = explosionViewHeight;
            // 应用到相机本地坐标
            mainCamera.transform.localPosition = localTarget;
        
            // 转换旋转
            mainCamera.transform.localRotation = Quaternion.Inverse(parent.rotation) * targetRotation;
        
            // Debug.Log($"转换后的本地位置: {localTarget}, 原始本地位置: {mainCamera.transform.localPosition}");
        }
        else
        {
            // 直接设置世界坐标（如果没有父对象）
            if (smoothTransition)
            {
                isTransitioning = true;
            }
            else
            {
                mainCamera.transform.position = targetPosition;
                mainCamera.transform.rotation = targetRotation;
            }
        }

        // 显示PIP
        pipDisplay.gameObject.SetActive(true);
    
        // 设置状态
        isShowingExplosion = true;
        explosionTimer = explosionDuration;
    }


    private void EndExplosionView()
    {
        if (smoothTransition)
        {
            // 平滑过渡回原始位置
            returnCoroutine = StartCoroutine(SmoothReturnToOriginal());
        }
        else
        {
            // 立即恢复主相机原始位置和方向
            mainCamera.transform.position = originalPosition;
            mainCamera.transform.rotation = originalRotation;
            
            CompleteExplosionEnd();
        }
    }
    
    private IEnumerator SmoothReturnToOriginal()
    {
        float t = 0;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        
        while (t < 1)
        {
            t += Time.deltaTime * transitionSpeed * 0.5f; // 回到原点可以稍慢一些
            mainCamera.transform.position = Vector3.Lerp(startPos, originalPosition, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, originalRotation, t);
            yield return null;
        }
        
        CompleteExplosionEnd();
        returnCoroutine = null;
    }
    
    private void CompleteExplosionEnd()
    {
        // 重新启用相机控制脚本
        if (cameraControl != null)
        {
            cameraControl.enabled = true;
        }

        // 关闭PIP显示
        pipDisplay.gameObject.SetActive(false);

        // 重置状态
        isShowingExplosion = false;
        isTransitioning = false;
    }
}
// using System.Collections;
// using UnityEngine;
// using UnityEngine.UI;
//
// public class ExplosionCamera : MonoBehaviour
// {
//     [Header("相机设置")]
//     [SerializeField] private Camera mainCamera;       // 主相机
//     [SerializeField] private Camera pipCamera;        // 画中画相机
//     [SerializeField] private RenderTexture pipRenderTexture;  // 画中画渲染纹理
//     [SerializeField] private RawImage pipDisplay;     // 画中画显示UI
//
//     [Header("爆炸视角设置")]
//     [SerializeField] private float explosionViewDistance = 20f;  // 相机与爆炸点的距离
//     [SerializeField] private float explosionViewAngle = 45f;    // 相机俯视角度
//     [SerializeField] private float explosionDuration = 3f;      // 爆炸视角持续时间
//     [SerializeField] private float transitionSpeed = 5f;        // 相机视角转换速度
//
//     private Vector3 currentExplosionPosition;         // 当前爆炸位置
//     private bool isShowingExplosion = false;          // 是否正在显示爆炸
//     private float explosionTimer = 0f;                // 爆炸计时器
//     private Transform originalCameraParent;           // 原始相机父对象
//     private Vector3 originalLocalPosition;            // 原始本地位置
//     private Quaternion originalLocalRotation;         // 原始本地旋转
//
//     private Coroutine lookAtCoroutine;                // LookAt协程引用
//
//     private void Start()
//     {
//         // 设置画中画相机
//         if (pipCamera != null && pipRenderTexture != null)
//         {
//             pipCamera.targetTexture = pipRenderTexture;
//         }
//
//         // 设置画中画显示
//         if (pipDisplay != null && pipRenderTexture != null)
//         {
//             pipDisplay.texture = pipRenderTexture;
//             pipDisplay.gameObject.SetActive(false);  // 初始隐藏画中画
//         }
//
//         // 存储主相机原始信息
//         if (mainCamera != null)
//         {
//             originalCameraParent = mainCamera.transform.parent;
//             originalLocalPosition = mainCamera.transform.localPosition;
//             originalLocalRotation = mainCamera.transform.localRotation;
//         }
//     }
//
//     private void Update()
//     {
//         // 处理爆炸视角持续时间
//         if (isShowingExplosion)
//         {
//             explosionTimer -= Time.deltaTime;
//             if (explosionTimer <= 0)
//             {
//                 EndExplosionView();
//             }
//         }
//     }
//
//     // 触发爆炸视角
//     public void TriggerExplosion(Vector3 explosionPosition)
//     {
//         // 保存爆炸位置
//         currentExplosionPosition = explosionPosition;
//         Debug.Log($"爆炸位置: {explosionPosition}");
//
//         // 如果已经在显示爆炸，重新聚焦到新爆炸
//         if (isShowingExplosion)
//         {
//             // 如果有正在进行的LookAt协程，停止它
//             if (lookAtCoroutine != null)
//             {
//                 StopCoroutine(lookAtCoroutine);
//             }
//             
//             // 开始新的LookAt协程
//             lookAtCoroutine = StartCoroutine(SmoothLookAtExplosion());
//         }
//         else
//         {
//             // 开始显示新爆炸
//             StartExplosionView();
//         }
//     }
//
//     // 开始爆炸视角
//     private void StartExplosionView()
//     {
//         if (mainCamera == null) return;
//
//         // 存储原始相机信息
//         originalCameraParent = mainCamera.transform.parent;
//         originalLocalPosition = mainCamera.transform.localPosition;
//         originalLocalRotation = mainCamera.transform.localRotation;
//
//         // 启用画中画显示
//         if (pipDisplay != null)
//         {
//             pipDisplay.gameObject.SetActive(true);
//         }
//
//         // 计算理想的观察位置
//         Vector3 viewPosition = CalculateIdealViewPosition();
//         mainCamera.transform.position = viewPosition;
//
//         // 开始LookAt协程
//         lookAtCoroutine = StartCoroutine(SmoothLookAtExplosion());
//
//         // 设置爆炸持续时间
//         isShowingExplosion = true;
//         explosionTimer = explosionDuration;
//     }
//
//     // 计算理想的观察位置
//     private Vector3 CalculateIdealViewPosition()
//     {
//         // 计算相机相对于爆炸点的位置
//         float angleInRadians = explosionViewAngle * Mathf.Deg2Rad;
//         float height = Mathf.Sin(angleInRadians) * explosionViewDistance;
//         float horizontalDistance = Mathf.Cos(angleInRadians) * explosionViewDistance;
//         
//         // 使用一个固定方向（例如北方）作为观察方向
//         Vector3 viewDirection = new Vector3(0, 0, -1);
//         
//         // 计算最终位置
//         Vector3 horizontalOffset = viewDirection.normalized * horizontalDistance;
//         return new Vector3(
//             currentExplosionPosition.x + horizontalOffset.x,
//             currentExplosionPosition.y + height,
//             currentExplosionPosition.z + horizontalOffset.z
//         );
//     }
//
//     // 平滑LookAt爆炸位置
//     private IEnumerator SmoothLookAtExplosion()
//     {
//         if (mainCamera == null) yield break;
//
//         // 计算目标旋转
//         Vector3 directionToTarget = currentExplosionPosition - mainCamera.transform.position;
//         Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
//         
//         // 平滑旋转
//         float t = 0;
//         Quaternion startRotation = mainCamera.transform.rotation;
//         
//         while (t < 1f)
//         {
//             t += Time.deltaTime * transitionSpeed;
//             mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
//             yield return null;
//         }
//         
//         // 确保最终旋转正确
//         mainCamera.transform.LookAt(currentExplosionPosition);
//     }
//
//     // 结束爆炸视角
//     private void EndExplosionView()
//     {
//         if (!isShowingExplosion || mainCamera == null) return;
//
//         // 停止任何正在进行的相机协程
//         if (lookAtCoroutine != null)
//         {
//             StopCoroutine(lookAtCoroutine);
//             lookAtCoroutine = null;
//         }
//
//         // 重置相机到原始位置和旋转
//         StartCoroutine(SmoothResetCamera());
//
//         // 隐藏画中画显示
//         if (pipDisplay != null)
//         {
//             pipDisplay.gameObject.SetActive(false);
//         }
//
//         isShowingExplosion = false;
//     }
//
//     // 平滑重置相机到原始状态
//     private IEnumerator SmoothResetCamera()
//     {
//         if (mainCamera == null) yield break;
//
//         Vector3 startPosition = mainCamera.transform.position;
//         Quaternion startRotation = mainCamera.transform.rotation;
//         
//         // 计算目标位置和旋转
//         Vector3 targetPosition = originalCameraParent.TransformPoint(originalLocalPosition);
//         Quaternion targetRotation = originalCameraParent.rotation * originalLocalRotation;
//         
//         float t = 0;
//         while (t < 1f)
//         {
//             t += Time.deltaTime * transitionSpeed;
//             mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
//             mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
//             yield return null;
//         }
//         
//         // 确保最终位置和旋转正确
//         mainCamera.transform.localPosition = originalLocalPosition;
//         mainCamera.transform.localRotation = originalLocalRotation;
//     }
//
//     // 强制结束爆炸视角
//     public void ForceEndExplosionView()
//     {
//         EndExplosionView();
//     }
// }
