using RD.Core;
using UnityEngine;

namespace RD.Client
{
    [EntitySystemOf(typeof(OperaComponent))]
    public static partial class OperaComponentSystem
    {
        [EntitySystem]
        private static void Awake(this OperaComponent self)
        {
            self.mapMask = LayerMask.GetMask("Map");
        }

        /// <summary>UE5 编辑器风格视口控制:
        /// 按住右键拖拽 = 转视角; 按住右键时 WASD = 前后左右, Q/E = 下降/上升, Shift = 加速;
        /// 滚轮 = 沿视线推进/后退; 左键点击 = 寻路移动</summary>
        [EntitySystem]
        private static void Update(this OperaComponent self)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                UpdateViewportControl(self, cam);
            }

            // 左键: 射线寻路移动
            if (cam != null && Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000, self.mapMask))
                {
                    C2M_PathfindingResult c2MPathfindingResult = C2M_PathfindingResult.Create();
                    c2MPathfindingResult.Position = hit.point;
                    self.Root().GetComponent<ClientSenderComponent>().Send(c2MPathfindingResult);
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                CodeLoader.Instance.Reload();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                C2M_TransferMap c2MTransferMap = C2M_TransferMap.Create();
                self.Root().GetComponent<ClientSenderComponent>().Call(c2MTransferMap).NoContext();
            }

            // 以下为框架功能演示快捷键, 与相机控制键(W/A/Q)冲突, 仅在未按住右键时生效
            if (!self.lookHeld)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    self.Test1().NoContext();
                }

                if (Input.GetKeyDown(KeyCode.W))
                {
                    self.Test2().NoContext();
                }

                if (Input.GetKeyDown(KeyCode.A))
                {
                    self.TestCancelAfter().WithContext(new PromiseCancellationToken());
                }
            }
        }

        private static void UpdateViewportControl(OperaComponent self, Camera cam)
        {
            if (Input.GetMouseButtonDown(1))
            {
                self.lookHeld = true;
                self.lastMousePos = Input.mousePosition;
                Vector3 e = cam.transform.eulerAngles;
                self.camYaw = e.y;
                self.camPitch = e.x > 180f ? e.x - 360f : e.x;
            }

            if (Input.GetMouseButtonUp(1))
            {
                self.lookHeld = false;
            }

            if (self.lookHeld)
            {
                Vector3 delta = Input.mousePosition - self.lastMousePos;
                self.lastMousePos = Input.mousePosition;
                self.camYaw += delta.x * 0.2f;
                self.camPitch -= delta.y * 0.2f;
                self.camPitch = Mathf.Clamp(self.camPitch, -89.9f, 89.9f);
                cam.transform.eulerAngles = new Vector3(self.camPitch, self.camYaw, 0);

                Vector3 move = Vector3.zero;
                if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
                if (Input.GetKey(KeyCode.S)) move -= Vector3.forward;
                if (Input.GetKey(KeyCode.D)) move += Vector3.right;
                if (Input.GetKey(KeyCode.A)) move -= Vector3.right;
                if (Input.GetKey(KeyCode.E)) move += Vector3.up;
                if (Input.GetKey(KeyCode.Q)) move -= Vector3.up;
                if (move != Vector3.zero)
                {
                    float speed = Input.GetKey(KeyCode.LeftShift) ? 60f : 20f;
                    cam.transform.position += Quaternion.Euler(self.camPitch, self.camYaw, 0) * move * (speed * Time.deltaTime);
                }
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                cam.transform.position += cam.transform.forward * (scroll * 200f);
            }
        }

        private static async Promise Test1(this OperaComponent self)
        {
            Log.Debug($"Croutine 1 start1 ");
            using (await self.Root().GetComponent<CoroutineLockComponent>().Wait(1, 20000, 3000))
            {
                await self.Root().GetComponent<TimerComponent>().WaitAsync(6000);
            }

            Log.Debug($"Croutine 1 end1");
        }
            
        private static async Promise Test2(this OperaComponent self)
        {
            PromiseCancellationToken oldCancellationToken = await PromiseHelper.GetContextAsync<PromiseCancellationToken>();
            Log.Debug($"Croutine 2 start2");
            using (await self.Root().GetComponent<CoroutineLockComponent>().Wait(1, 20000, 3000))
            {
                await self.Root().GetComponent<TimerComponent>().WaitAsync(1000);
            }
            Log.Debug($"Croutine 2 end2");
        }
        
        private static async Promise TestCancelAfter(this OperaComponent self)
        {
            PromiseCancellationToken oldCancellationToken = await PromiseHelper.GetContextAsync<PromiseCancellationToken>();
            
            Log.Debug($"TestCancelAfter start");
            PromiseCancellationToken newCancellationToken = new();
            await self.Root().GetComponent<TimerComponent>().WaitAsync(3000).TimeoutAsync(newCancellationToken, 1000);
            if (newCancellationToken.IsCancel())
            {
                Log.Debug($"TestCancelAfter newCancellationToken is cancel!");
            }
            
            if (oldCancellationToken != null && !oldCancellationToken.IsCancel())
            {
                Log.Debug($"TestCancelAfter oldCancellationToken is not cancel!");
            }
            Log.Debug($"TestCancelAfter end");
        }
    }
}