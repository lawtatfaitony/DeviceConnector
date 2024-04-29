using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VideoGuard.ApiModels;
using X.PagedList;
using VxGuardClient.Context;
using VideoGuard.ApiModels.Task;
using Task = VideoGuard.ApiModels.Task.Task;
using VideoGuard.ApiModels.CamApiModel;
using VideoGuard.ApiModels.LibraryApiModel;
using Type = VideoGuard.ApiModels.Task.Type;
using LogUtility;
using VxClient.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VxGuardClient.Controllers
{
    public partial class TaskController : BaseController
    {
        public TaskController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IOptions<TokenManagement> tokenManagement)
           : base(webHostEnvironment, httpContextAccessor)
        {
            WebCookie.httpContextAccessor = httpContextAccessor;
            Logger = Logger;
        }
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public ActionResult AddTask()
        {
            ResponseModalX responseModalX = new ResponseModalX();
            MainCom mainCom = new MainCom();
            TaskModelInput taskModelInput = new TaskModelInput { Threshold = 0.8, Interval = 2, Type = TaskType.UNDEFINED };
            taskModelInput.MaincomId = WebCookie.MainComId ?? mainCom.MainComId;
            responseModalX.data = taskModelInput;
            return View(responseModalX);
        }
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult AddTask(TaskModelInput taskModelInput)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            if (string.IsNullOrEmpty(taskModelInput.Name))
            {
                responseModalX.meta.Success = false;
                responseModalX.meta.ErrorCode = (int)TaskErrorCode.TASK_ILLEGAL_NAME;
                responseModalX.meta.Message = Lang.TASK_ILLEGAL_NAME;
                responseModalX.data = null;
                return Ok(responseModalX);
            }
            using (BusinessContext businessContext = new BusinessContext())
            {
                DateTime dt = DateTime.Now;
                int maxId = 999;
                if (businessContext.FtTask.Count() > 0)
                {
                    maxId = businessContext.FtTask.Max(c => c.Id) + 1;
                }
                MainCom mainCom = new MainCom(); //默認值  
                FtTask ftTask = new FtTask
                {
                    Id = maxId,
                    MaincomId = taskModelInput.MaincomId ?? WebCookie.MainComId ?? mainCom.MainComId,
                    CameraList1 = taskModelInput.CameraList1,
                    CameraList2 = taskModelInput.CameraList2,
                    LibList = taskModelInput.LibList,
                    Name = taskModelInput.Name,
                    Type = Convert.ToSByte(taskModelInput.Type),
                    Visible = 1,
                    Threshold = (float)taskModelInput.Threshold,
                    State = Convert.ToSByte(1),
                    Plan = taskModelInput.Plan,
                    Interval = taskModelInput.Interval,
                    CreateTime = dt,
                    UpdateTime = dt,
                    Remark = taskModelInput.Remark
                };
                businessContext.FtTask.Add(ftTask);
                bool result = businessContext.SaveChanges() > 0 ? true : false;
                if (result)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)TaskErrorCode.TASK_ADD_SUCCESS, Message = Lang.TASK_ADD_SUCCESS, Success = true },
                        data = ftTask
                    };
                    OkObjectResult okObjectResult = Ok(responseModalX);
                    return okObjectResult;
                }
                else
                {
                    MetaModalX metaModalX = new MetaModalX
                    {
                        ErrorCode = (int)TaskErrorCode.TASK_ADD_FAIL,
                        Success = false,
                        Message = TaskErrorCode.TASK_ADD_FAIL.GetDescriptionX()
                    };
                    responseModalX = new ResponseModalX
                    {
                        meta = metaModalX,
                        data = null
                    };
                    OkObjectResult okObjectResult = Ok(responseModalX);
                    return okObjectResult;
                }
            }
        }

        /// <summary>
        /// Query Task List By Searching factor
        /// </summary>
        /// <param name="queryTaskListInput"></param>
        /// <returns></returns>
        [Authorize]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public IActionResult QueryTaskList([FromQuery] QueryTaskListInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            if (input.PageNo == 0)
            {
                MetaModalX metaModalX = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_PAGE_NO_ERR, Message = Lang.GeneralUI_PAGE_NO_ERR };
                responseModalX.meta = metaModalX;
                responseModalX.data = null;
                return SwitchToApiOrView(responseModalX);
            }
            if (!string.IsNullOrEmpty(input.Name))
            {
                input.Name = Uri.UnescapeDataString(input.Name).Trim();
            }

            ViewBag.QueryTaskListInput = input;

            QueryTaskListInfoReturn queryTaskListInfoReturn = new QueryTaskListInfoReturn();
            List<Task> items = new List<Task>();
            try
            {
                using (BusinessContext businessContext = new BusinessContext())
                {
                    var ftTasks = businessContext.FtTask.Where(c => c.MaincomId.Contains(WebCookie.MainComId) && c.Visible == (sbyte)TaskErrorCode.TASK_IS_VISIBLE).ToList();
                    if (!string.IsNullOrEmpty(input.Name))
                    {
                        ftTasks = ftTasks.Where(c => c.Name.Contains(input.Name)).ToList();
                    }
                    if (input.Type != 0)
                    {
                        ftTasks = ftTasks.Where(c => c.Type == input.Type).ToList();
                    }
                    foreach (var item in ftTasks)
                    {
                        List<CameraList> cameraLists1 = new List<CameraList>();
                        List<CameraList> cameraLists2 = new List<CameraList>();
                        if (item.CameraList1.Split(',').Length > 0)
                        {
                            foreach (var cameraItemId in item.CameraList1.Split(','))
                            {
                                bool bTryCameraId = int.TryParse(cameraItemId, out int id);
                                if (bTryCameraId)
                                {
                                    FtCamera ftCamera = businessContext.FtCamera.Find(id);
                                    if (ftCamera != null)
                                    {
                                        CameraList cam = new CameraList
                                        {
                                            CameraId = ftCamera.Id,
                                            Name = ftCamera.Name,
                                            State = (sbyte)ftCamera.Visible
                                        };
                                        cameraLists1.Add(cam);
                                    }
                                }
                            }
                        }
                        else
                        {
                            cameraLists1 = null;
                        }

                        if (item.CameraList2.Split(',').Length > 0)
                        {
                            foreach (var cameraItemId in item.CameraList2.Split(','))
                            {
                                bool bTryCameraId = int.TryParse(cameraItemId, out int id);
                                if (bTryCameraId)
                                {
                                    FtCamera ftCamera = businessContext.FtCamera.Find(id);
                                    if (ftCamera != null)
                                    {
                                        CameraList cam = new CameraList
                                        {
                                            CameraId = ftCamera.Id,
                                            Name = ftCamera.Name,
                                            State = (sbyte)ftCamera.Visible
                                        };
                                        cameraLists2.Add(cam);
                                    }
                                }
                            }
                        }
                        else
                        {
                            cameraLists2 = null;
                        }

                        Task taskItem = new Task
                        {
                            TaskId = item.Id,
                            Name = item.Name,
                            Type = (sbyte)item.Type,
                            CameraList1 = cameraLists1,
                            CameraList2 = cameraLists2,
                            LibList = item.LibList,
                            LibIdGroupsList = GetLibIdGroupsList(item.LibList),
                            Interval = (sbyte)item.Interval.GetValueOrDefault(),
                            Threshold = (double)item.Threshold.GetValueOrDefault(),
                            State = (sbyte)item.State.GetValueOrDefault(),
                            Plan = item.Plan,
                            Remark = item.Remark,
                            CreateTime = string.Format("{0:yyyy-MM-ddTHH:mm:ss}", item.CreateTime)
                        };
                        items.Add(taskItem);
                    }
                    var newItems = items.ToPagedList(input.PageNo, input.PageSize);
                    queryTaskListInfoReturn.PageCount = newItems.PageCount;
                    queryTaskListInfoReturn.PageNo = newItems.PageNumber;
                    queryTaskListInfoReturn.PageSize = newItems.PageSize;
                    queryTaskListInfoReturn.TotalCount = newItems.Count();
                    queryTaskListInfoReturn.Items = newItems.ToList();

                    responseModalX.data = queryTaskListInfoReturn;
                    return SwitchToApiOrView(responseModalX);
                };
            }
            catch (Exception ex)
            {
                LogHelper.Error($"[QueryTaskList :: {Lang.TASK_LIST_FAIL}] [ErrorCode = {TaskErrorCode.TASK_LIST_FAIL}] [Exception][{ex.Message}][line 3244.fhsk3]");
                MetaModalX metaModalX = new MetaModalX { ErrorCode = (int)TaskErrorCode.TASK_LIST_FAIL, Success = false, Message = $"{Lang.TASK_LIST_FAIL} [Exception][{ex.Message}]" };
                responseModalX.meta = metaModalX;
                responseModalX.data = null;
                return SwitchToApiOrView(responseModalX);
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult UpdateTask(int taskId)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            using (BusinessContext businessContext = new BusinessContext())
            {
                FtTask ftTask = businessContext.FtTask.Find(taskId);
                if (ftTask != null)
                {
                    TaskUpdateInput taskUpdateInput = new TaskUpdateInput
                    {
                        TaskId = ftTask.Id,
                        MaincomId = ftTask.MaincomId,
                        Name = ftTask.Name,
                        Type = (TaskType)ftTask.Type.GetValueOrDefault(),
                        CameraList1 = string.IsNullOrEmpty(ftTask.CameraList1) ? "" : ftTask.CameraList1,
                        CameraList2 = string.IsNullOrEmpty(ftTask.CameraList2) ? "" : ftTask.CameraList2,
                        Interval = ftTask.Interval.GetValueOrDefault(),
                        LibList = ftTask.LibList,
                        Plan = ftTask.Plan,
                        Threshold = Math.Round(ftTask.Threshold.GetValueOrDefault(), 2),
                        Remark = ftTask.Remark,
                        session = WebCookie.ApiSession
                    };
                    responseModalX.data = taskUpdateInput;
                    return SwitchToApiOrView(responseModalX);
                }
                else
                {
                    responseModalX.meta.Success = false;
                    responseModalX.meta.ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD;
                    responseModalX.meta.Message = Lang.GeneralUI_NoRecord;
                    responseModalX.data = null;
                    return Ok(responseModalX);
                }
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public IActionResult GetCameraList()
        {
            ResponseModalX responseModalX = new ResponseModalX();
            try
            {
                using (BusinessContext businessContext = new BusinessContext())
                {
                    // List<Camera> cameras = new List<Camera>();
                    var ftCameras = businessContext.FtCamera.Where(c => c.Visible == (int)GeneralVisible.VISIBLE).ToList();

                    List<QueryCameraListSelect> queryCameraListSelects = new List<QueryCameraListSelect>();
                    foreach (var item in ftCameras)
                    {
                        QueryCameraListSelect queryCameraListSelect = new QueryCameraListSelect { label = item.Name, value = item.Id.ToString() };
                        queryCameraListSelects.Add(queryCameraListSelect);
                    }
                    responseModalX.data = queryCameraListSelects;
                    return Ok(responseModalX);
                }
            }
            catch (Exception ex)
            {
                responseModalX.meta = new MetaModalX
                {
                    Success = false,
                    Message = string.Format("{0}-{1}", Lang.GeneralUI_Fail, ex.Message),
                    ErrorCode = (int)GeneralReturnCode.EXCEPTION
                };
                return Ok(responseModalX);
            }
        }

        //QueryLibraryListSelect
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]/{mainComId}")]
        [HttpGet]
        public IActionResult QueryLibraryListSelect(string mainComId)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            try
            {
                using (BusinessContext businessContext = new BusinessContext())
                {
                    List<FtLibrary> ftLibrarys = businessContext.FtLibrary.Where(c => c.MaincomId.Contains(mainComId) && c.Visible == (int)LibraryErrorCode.LIB_IS_VISIBLE).ToList();
                    List<QueryLibListSelect> queryLibListSelects = new List<QueryLibListSelect>();

                    foreach (var item in ftLibrarys)
                    {
                        QueryLibListSelect QueryLibListSelect1 = new QueryLibListSelect { label = item.Name, value = item.Id.ToString() };
                        queryLibListSelects.Add(QueryLibListSelect1);
                    }
                    responseModalX.data = queryLibListSelects;
                    return Ok(responseModalX);
                }
            }
            catch (Exception ex)
            {
                responseModalX.meta = new MetaModalX
                {
                    Success = false,
                    Message = string.Format("{0}-{1}", Lang.GeneralUI_Fail, ex.Message),
                    ErrorCode = (int)GeneralReturnCode.EXCEPTION
                };
                return Ok(responseModalX);
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult UpdateTask(TaskUpdateInput taskUpdateInput)
        {
            DateTime dt = DateTime.Now;
            ResponseModalX responseModalX = new ResponseModalX();
            if (string.IsNullOrEmpty(taskUpdateInput.Name))
            {
                responseModalX.meta.Success = false;
                responseModalX.meta.ErrorCode = (int)TaskErrorCode.TASK_ILLEGAL_NAME;
                responseModalX.meta.Message = Lang.TASK_ILLEGAL_NAME;
                responseModalX.data = null;
                return Ok(responseModalX);
            }

            if (string.IsNullOrEmpty(taskUpdateInput.CameraList1))
            {
                responseModalX.meta.Success = false;
                responseModalX.meta.ErrorCode = (int)TaskErrorCode.TASK_UPDATE_FAIL;
                responseModalX.meta.Message = "Please re-select the camera list 1.It is required!";
                responseModalX.data = null;
                return Ok(responseModalX);
            }
            if (string.IsNullOrEmpty(taskUpdateInput.CameraList2))
            {
                responseModalX.meta.Success = false;
                responseModalX.meta.ErrorCode = (int)TaskErrorCode.TASK_UPDATE_FAIL;
                responseModalX.meta.Message = "Please re-select the camera list 2 , the default vaue is required!";
                responseModalX.data = null;
                return Ok(responseModalX);
            }

            if (string.IsNullOrEmpty(taskUpdateInput.LibList))
            {
                responseModalX.meta.Success = false;
                responseModalX.meta.ErrorCode = (int)TaskErrorCode.TASK_UPDATE_FAIL;
                responseModalX.meta.Message = "Please re-select the library list , it is required!";  //Lang.Task_LibList_Tips2
                responseModalX.data = null;
                return Ok(responseModalX);
            }

            using (BusinessContext businessContext = new BusinessContext())
            {
                FtTask ftTask = businessContext.FtTask.Find(taskUpdateInput.TaskId);
                if (ftTask != null)
                {
                    ftTask.CameraList1 = taskUpdateInput.CameraList1;
                    ftTask.CameraList2 = taskUpdateInput.CameraList2;
                    ftTask.LibList = taskUpdateInput.LibList;
                    ftTask.Name = taskUpdateInput.Name;
                    ftTask.Type = Convert.ToSByte(taskUpdateInput.Type);
                    ftTask.Threshold = (float)taskUpdateInput.Threshold;
                    ftTask.State = Convert.ToSByte(1);
                    ftTask.Plan = taskUpdateInput.Plan;
                    ftTask.Interval = taskUpdateInput.Interval;
                    ftTask.UpdateTime = dt;
                    ftTask.Remark = taskUpdateInput.Remark;

                    businessContext.FtTask.Update(ftTask);
                    bool result = businessContext.SaveChanges() > 0;
                    if (result)
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { ErrorCode = (int)TaskErrorCode.TASK_ADD_SUCCESS, Message = Lang.TASK_UPDATE_SUCCESS, Success = true },
                            data = ftTask
                        };

                        return Ok(responseModalX);
                    }
                    else
                    {
                        MetaModalX metaModalX = new MetaModalX
                        {
                            ErrorCode = (int)TaskErrorCode.TASK_UPDATE_FAIL,
                            Success = false,
                            Message = TaskErrorCode.TASK_UPDATE_FAIL.GetDescriptionX()
                        };
                        responseModalX = new ResponseModalX
                        {
                            meta = metaModalX,
                            data = null
                        };
                        return Ok(responseModalX);
                    }
                }
                else
                {
                    responseModalX.meta.Success = false;
                    responseModalX.meta.ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD;
                    responseModalX.meta.Message = Lang.GeneralUI_NoRecord;
                    responseModalX.data = null;
                    return Ok(responseModalX);
                }
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult GetTask(TaskIdInput taskIdInput)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            //CameraList cameraListOfNothing = new CameraList { CameraId = 0, State = 0, Name = "NO CAMERA" };
            //List<CameraList> noCameraList = new List<CameraList>();
            //noCameraList.Add(cameraListOfNothing);

            using (BusinessContext businessContext = new BusinessContext())
            {
                FtTask ftTask = businessContext.FtTask.Find(taskIdInput.TaskId);
                if (ftTask != null)
                {
                    Task task = new Task
                    {
                        TaskId = ftTask.Id,
                        CameraList1 = null,
                        CameraList2 = null,
                        Type = ftTask.Type.GetValueOrDefault(),
                        Name = ftTask.Name,
                        Interval = ftTask.Interval.GetValueOrDefault(),
                        Threshold = ftTask.Threshold.GetValueOrDefault(),
                        LibList = ftTask.LibList,
                        State = ftTask.State.GetValueOrDefault(),
                        Plan = ftTask.Plan,
                        Remark = ftTask.Remark,
                        CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", ftTask.CreateTime)
                    };
                    List<CameraList> cameraLists1 = new List<CameraList>();
                    if (!string.IsNullOrEmpty(ftTask.CameraList1))
                    {
                        string[] camArrStr = ftTask.CameraList1.Split(',');
                        if (camArrStr.Length > 0)
                        {
                            foreach (string item in camArrStr)
                            {
                                FtCamera ftCamera = businessContext.FtCamera.Find(int.Parse(item));
                                if (ftCamera != null)
                                {
                                    CameraList cameraList = new CameraList { CameraId = ftCamera.Id, Name = ftCamera.Name, State = ftCamera.Visible.GetValueOrDefault() };
                                    cameraLists1.Add(cameraList);
                                }
                            }
                        }
                    }
                    List<CameraList> cameraLists2 = new List<CameraList>();
                    if (!string.IsNullOrEmpty(ftTask.CameraList2))
                    {
                        string[] camArrStr = ftTask.CameraList2.Split(',');
                        if (camArrStr.Length > 0)
                        {
                            foreach (string item in camArrStr)
                            {
                                FtCamera ftCamera = businessContext.FtCamera.Find(int.Parse(item));
                                if (ftCamera != null)
                                {
                                    CameraList cameraList = new CameraList { CameraId = ftCamera.Id, Name = ftCamera.Name, State = ftCamera.Visible.GetValueOrDefault() };
                                    cameraLists2.Add(cameraList);
                                }
                            }
                        }
                    }

                    task.CameraList1 = cameraLists1;
                    task.CameraList2 = cameraLists2;

                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_UNAUTHORIED },
                        data = task
                    };
                    OkObjectResult okObjectResult = Ok(responseModalX);
                    return okObjectResult;
                }
                else
                {
                    responseModalX.meta.Success = false;
                    responseModalX.meta.ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD;
                    responseModalX.meta.Message = Lang.GeneralUI_NoRecord;
                    responseModalX.data = null;
                    return Ok(responseModalX);
                }

            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult StartTask(StartTaskModeInput startTaskModeInput)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            using (BusinessContext businessContext = new BusinessContext())
            {
                FtTask ftTask = businessContext.FtTask.Find(startTaskModeInput.TaskId);
                if (ftTask != null)
                {
                    ftTask.State = (sbyte)TaskErrorCode.TASK_START_IN_VALUE;
                    businessContext.FtTask.Update(ftTask);

                    bool result = businessContext.SaveChanges() > 0 ? true : false;
                    if (result)
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { ErrorCode = (int)TaskErrorCode.TASK_START_IN_VALUE, Message = Lang.TASK_START_STATUS_DESC, Success = true },
                            data = ftTask
                        };
                        OkObjectResult okObjectResult = Ok(responseModalX);
                        return okObjectResult;
                    }
                    else
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { ErrorCode = (int)TaskErrorCode.TASK_START_FAIL, Success = false, Message = TaskErrorCode.TASK_START_FAIL.GetDescriptionX() },
                            data = null
                        };
                        OkObjectResult okObjectResult = Ok(responseModalX);
                        return okObjectResult;
                    }
                }
                else
                {
                    responseModalX.meta.Success = false;
                    responseModalX.meta.ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD;
                    responseModalX.meta.Message = Lang.GeneralUI_NoRecord;
                    responseModalX.data = null;
                    return Ok(responseModalX);
                }
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult StopTask(StopTaskModeInput stopTaskModeInput)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            using (BusinessContext businessContext = new BusinessContext())
            {
                FtTask ftTask = businessContext.FtTask.Find(stopTaskModeInput.TaskId);
                if (ftTask != null)
                {
                    ftTask.State = (sbyte)TaskErrorCode.TASK_STOP_IN_VALUE;
                    businessContext.FtTask.Update(ftTask);
                    bool result = businessContext.SaveChanges() > 0 ? true : false;
                    if (result)
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { ErrorCode = (int)TaskErrorCode.TASK_STOP_IN_VALUE, Message = Lang.TASK_STOP_STATUS_DESC, Success = true },
                            data = ftTask
                        };
                        OkObjectResult okObjectResult = Ok(responseModalX);
                        return okObjectResult;
                    }
                    else
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { ErrorCode = (int)TaskErrorCode.TASK_STOP_FAIL, Success = false, Message = TaskErrorCode.TASK_STOP_FAIL.GetDescriptionX() },
                            data = null
                        };
                        OkObjectResult okObjectResult = Ok(responseModalX);
                        return okObjectResult;
                    }
                }
                else
                {
                    responseModalX.meta.Success = false;
                    responseModalX.meta.ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD;
                    responseModalX.meta.Message = Lang.GeneralUI_NoRecord;
                    responseModalX.data = null;
                    return Ok(responseModalX);
                }
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult DeleteTask(TaskDeleteModeInput taskDeleteModeInput)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            using (BusinessContext businessContext = new BusinessContext())
            {
                FtTask ftTask = businessContext.FtTask.Find(taskDeleteModeInput.TaskId);
                if (ftTask != null)
                {
                    ftTask.Visible = (sbyte)TaskErrorCode.TASK_DELETED;
                    businessContext.FtTask.Update(ftTask);
                    bool result = businessContext.SaveChanges() > 0 ? true : false;
                    if (result)
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { ErrorCode = (int)TaskErrorCode.TASK_DELETE_SUCCESS, Message = Lang.TASK_DEL_STATUS_NOT_VISIBLE, Success = true },
                            data = ftTask
                        };
                        OkObjectResult okObjectResult = Ok(responseModalX);
                        return okObjectResult;
                    }
                    else
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { ErrorCode = (int)TaskErrorCode.TASK_DELETE_FAIL, Success = false, Message = TaskErrorCode.TASK_DELETE_FAIL.GetDescriptionX() },
                            data = null
                        };
                        OkObjectResult okObjectResult = Ok(responseModalX);
                        return okObjectResult;
                    }
                }
                else
                {
                    responseModalX.meta.Success = false;
                    responseModalX.meta.ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD;
                    responseModalX.meta.Message = Lang.GeneralUI_NoRecord;
                    responseModalX.data = null;
                    return Ok(responseModalX);
                }
            }
        }


        /// <summary>
        /// 獲取當前查詢鏡頭的任務，規則：每個任務只列最近更新的一條。
        /// Task_GetTaskListByCameraId_Tips
        /// </summary>
        /// <param name="cameraId"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]/{cameraId}")]
        [HttpGet]
        public IActionResult GetTaskListByCameraId(int cameraId)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            var taskTypeQuery = HtmlEnumListExtend.GetEnumSelectList<TaskType>();

            using (BusinessContext businessContext = new BusinessContext())
            {
                var camera = businessContext.FtCamera.Find(cameraId);
                if (camera == null)
                {
                    responseModalX.meta = new MetaModalX { Success = false, Message = $"NOT EXIST THIS CAMERA (CAMERA ID = {cameraId}) TIPS:{Lang.Task_GetTaskListByCameraId_Tips}", ErrorCode = (int)GeneralReturnCode.LIST_NO_RECORD };
                    return Ok(responseModalX);
                }
                string cameraIdStr = camera.Id.ToString();
                var allTaskListByCameraId = businessContext.FtTask.Where(c => c.CameraList1.Contains(cameraIdStr) || c.CameraList2.Contains(cameraIdStr) && c.State == (sbyte)TaskErrorCode.TASK_START_IN_VALUE);

                if (allTaskListByCameraId.Count() > 0)
                {
                    List<FtTask> taskList = new List<FtTask>();
                    foreach (var enum_item in taskTypeQuery)
                    {
                        TaskType taskType = Enum.Parse<TaskType>(enum_item.Value);
                        sbyte typeValue = Convert.ToSByte(taskType);

                        //獲取更新的第一條
                        var taskItem = allTaskListByCameraId.Where(c => c.Type.Value == typeValue).OrderByDescending(c => c.UpdateTime).FirstOrDefault();

                        if (taskItem == null)
                            continue;

                        taskList.Add(taskItem);
                    }

                    if (taskList.Count() > 0)
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { Success = true, ErrorCode = (int)TaskErrorCode.TASK_DELETE_SUCCESS, Message = $"GET CAMERA TASK SUCCESS,BY CAMERA ID ={cameraIdStr} TIPS:{Lang.Task_GetTaskListByCameraId_Tips}" },
                            data = taskList
                        };
                        OkObjectResult okObjectResult = Ok(responseModalX);
                        return okObjectResult;
                    }
                    else
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { ErrorCode = (int)TaskErrorCode.TASK_LIST_FAIL, Success = false, Message = $"GET CAMERA TASK FAIL,BY CAMERA ID ={cameraIdStr} TIPS:{Lang.Task_GetTaskListByCameraId_Tips}" },
                            data = null
                        };
                        OkObjectResult okObjectResult = Ok(responseModalX);
                        return okObjectResult;
                    }
                }
                else
                {
                    responseModalX.meta.Success = false;
                    responseModalX.meta.ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD;
                    responseModalX.meta.Message = $"{Lang.LIST_NO_RECORD}; All Task List By CameraId";
                    responseModalX.data = null;
                    return Ok(responseModalX);
                }
            }
        }


        public List<FtLibrary> GetLibIdGroupsList(string libIdGroups)
        {
            if (string.IsNullOrEmpty(libIdGroups))
                return null;

            if (libIdGroups.EndsWith(","))
            {
                libIdGroups = libIdGroups.TrimEnd(',');
            }

            if (libIdGroups.StartsWith(","))
            {
                libIdGroups = libIdGroups.TrimStart(',');
            }

            string[] libIdGroupsArray = libIdGroups.Split(",");

            if (libIdGroupsArray.Length == 0)
                return null;

            using BusinessContext businessContext = new BusinessContext();

            List<FtLibrary> libraries = new List<FtLibrary>();

            foreach (var item in libIdGroupsArray)
            {
                int libId = int.Parse(item);
                var library = businessContext.FtLibrary.Find(libId);
                if (library != null)
                    libraries.Add(library);
            }

            return libraries;
        }
    }
}
