using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Linq;
using Common;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VideoGuard.ApiModels; 
using static EnumCode.EnumBusiness;
using System.IO;
using System.Collections;

namespace VideoGuard.ApiModels
{
    public partial class PersonBusiness
    {
        public static readonly string[] SubFolderLimited = new string[] { "Person", "CropPerson", "Mpeg" };

        /// <summary>
        /// 通過外部工號返回人員資料
        /// 注意外部工號可以是 Ftperson.Id (字符串形式傳入)
        /// </summary>
        /// <param name="outerId"></param>
        /// <param name="ftPerson"></param>
        /// <param name="responseModalX"></param>
        /// <param name="return">返回true表示存在</param>
        /// <returns></returns>
        public static bool CheckExistPersonByOuterId(string maincomId,string outerId, ref FtPerson ftPerson, ref ResponseModalX responseModalX,ref string picPath)
        {
            ftPerson = new FtPerson();
            ftPerson = null; //预设
            responseModalX = new ResponseModalX
            {
                meta = new MetaModalX {Success=false,ErrorCode=(int)GeneralReturnCode.FAIL, Message = Lang.GeneralUI_NoRecord },
                data = null
            };
            using BusinessContext businessContext = new BusinessContext();
            ftPerson = businessContext.FtPerson.Where(c => c.OuterId.Contains(outerId) && c.MaincomId.Contains(maincomId)).FirstOrDefault();
            if (ftPerson != null)
            {
                responseModalX = new ResponseModalX();
                responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_SUCC };
                 
                bool isExistPicture = ChkValidOfPersonPicture(ftPerson.Id, out FtPicture ftPicture, out ResponseModalX responseModalX2);
                if(isExistPicture)
                {
                    picPath = ftPicture.PicUrl;
                }
                return true;
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_NOT_EXIST, Message = Lang.PERSON_NOT_EXIST },
                    data = null
                };
                return false;
            }
        }
         
        public static bool ChkValidOfPersonPicture(long personId, out FtPicture ftPicture, out ResponseModalX responseModalX)
        {

            using (BusinessContext businessContext = new BusinessContext())
            {
                ftPicture = businessContext.FtPicture.Where(c => c.PersonId == personId).OrderByDescending(c => c.UpdateTime).FirstOrDefault();
                if (ftPicture != null)
                {
                    responseModalX = new ResponseModalX();
                    responseModalX.meta = new MetaModalX {
                        Success = true, 
                        ErrorCode = (int)PictureErrorCode.PICTURE_EXIST,
                        Message = Lang.PICTURE_EXIST 
                    };
                }
                else
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { 
                            Success = false, 
                            ErrorCode = (int)PictureErrorCode.PICTURE_NOT_EXIST, 
                            Message =$"({Lang.Person_Id}:{personId}){Lang.PICTURE_NOT_EXIST}" 
                        },
                        data = null
                    };
                }
                return responseModalX.meta.Success;
            }
        }
        /// <summary>
        /// 把虛擬格式的圖片路徑轉 實體 /Files/download/Person/1659766650247
        /// </summary>
        /// <param name="virtualDownFile"></param>
        /// <returns></returns>
        public static string TransPersonActualPicture(string uploadFolderPath,string virtualDownFile)
        {
            if (string.IsNullOrEmpty(virtualDownFile))
                return virtualDownFile;

            virtualDownFile = virtualDownFile.ToLower();
 

            string[] splitArr = virtualDownFile.Split("/");
            string name = splitArr[splitArr.Length - 1];
            string tagetFilename = $"{name}.jpg";

            string pathPath;
            string personFolder = SubFolderLimited[0];
            if (long.TryParse(name,out long fileUpOccur))
            {
                DateTime occur = DateTimeHelp.ConvertToDateTime(fileUpOccur);
                string monthFolder = $"{occur:yyyyMM}";
                
                pathPath = Path.Combine(uploadFolderPath, personFolder, monthFolder, tagetFilename);

                if (File.Exists(pathPath))
                    return virtualDownFile;
                else
                    return string.Empty;
            }
            else
            {
                string monthFolder = $"{DateTime.Now:yyyyMM}"; ///看看本月的
                pathPath = Path.Combine(uploadFolderPath, personFolder, monthFolder, tagetFilename);
                if (File.Exists(pathPath))
                    return virtualDownFile;
                else
                    return string.Empty;
            } 
        }

        /// <summary>
        /// 不存在,則沒有同名 reurn = false
        /// 檢測姓名是否已經被佔用
        /// </summary>
        /// <param name="PersonName"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool ChkValidOfPersonName(string mainComId ,string personName, out ResponseModalX responseModalX)
        {
            if (!string.IsNullOrEmpty(personName))
            { 
                responseModalX = new ResponseModalX();

                using BusinessContext businessContext = new BusinessContext();

                var checkSame = businessContext.FtPerson.Where(c => c.MaincomId.Contains(mainComId) && c.Name == personName).Any();

                 //不存在,則沒有同名,
                if(checkSame==false)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_INVALID_NAME, Message = Lang.PERSON_INVALID_NAME },
                        data = null
                    };
                }
                else
                {
                    //PERSON_EXIST_THE_NAME
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = true, ErrorCode = (int)PersonErrorCode.PERSON_EXIST_THE_NAME, Message = Lang.PERSON_EXIST_THE_NAME },
                        data = null
                    };
                }
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_INVALID_NAME, Message = Lang.PERSON_INVALID_NAME },
                    data = null
                };
            }
            return responseModalX.meta.Success;
        }

        public static bool ChkValidOfPersonExist(long personId, out FtPerson ftPerson, out ResponseModalX responseModalX)
        {
            using (BusinessContext businessContext = new BusinessContext())
            { 
                ftPerson = businessContext.FtPerson.Find(personId);
                if (ftPerson != null)
                {
                    responseModalX = new ResponseModalX();
                    responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)PersonErrorCode.PERSON_EXIST_PERSON, Message = Lang.PERSON_EXIST_PERSON };
                }
                else
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_NOT_EXIST, Message = Lang.PERSON_NOT_EXIST },
                        data = null
                    };
                }
                return responseModalX.meta.Success;
            }
        }
        //傳入 工號或數字 可變類型查詢 人員詳細資料
        public static bool ChkValidOfPersonExist(string employeeId, out FtPerson ftPerson, out ResponseModalX responseModalX)
        {
            using (BusinessContext businessContext = new BusinessContext())
            {
                if (long.TryParse(employeeId,out long personId))
                {
                    ftPerson = businessContext.FtPerson.Find(personId);
                }else
                {
                    ftPerson = businessContext.FtPerson.Where(c=>c.OuterId.Contains(employeeId)).FirstOrDefault();
                }
                   
                if (ftPerson != null)
                {
                    responseModalX = new ResponseModalX();
                    responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)PersonErrorCode.PERSON_EXIST_PERSON, Message = Lang.PERSON_EXIST_PERSON };
                    responseModalX.data = ftPerson;
                }
                else
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_NOT_EXIST, Message = Lang.PERSON_NOT_EXIST },
                        data = null
                    };
                }
                return responseModalX.meta.Success;
            }
        }

        /// <summary>
        /// 人員庫ID是否有效
        /// </summary>
        /// <param name="personModelInput"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool ChkValidOfLibId(PersonModelInput personModelInput, out ResponseModalX responseModalX)
        {
            using (BusinessContext businessContext = new BusinessContext())
            {
                FtLibrary ftLibrary = businessContext.FtLibrary.Find(personModelInput.LibId);
                if (ftLibrary != null)
                {
                    responseModalX = new ResponseModalX();
                }
                else
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_INVLID_LIBRARY_ID, Message = Lang.PERSON_INVLID_LIBRARY_ID },
                        data = null
                    };
                }
                return responseModalX.meta.Success;
            }
        }
        /// <summary>
        /// 人員庫ID是否存在
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="ftLibrary"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool ChkValidOfLibIdExist(int libId, out FtLibrary ftLibrary, out ResponseModalX responseModalX)
        {
            using (BusinessContext businessContext = new BusinessContext())
            {
                ftLibrary = businessContext.FtLibrary.Find(libId);
                if (ftLibrary != null)
                {
                    responseModalX = new ResponseModalX();
                    responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_SUCC };
                }
                else
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)LibraryErrorCode.LIB_NOT_EXIST, Message = $"{Lang.GeneralUI_Fail} {Lang.GeneralUI_NoRecord} {Lang.Library_LibId}{libId}" },
                        data = null
                    };
                }
                return responseModalX.meta.Success;
            }
        }

        public static bool GetUrlPathFileNameId(string urlPath, out long fileNameId)
        {
            if (string.IsNullOrEmpty(urlPath))
            {
                fileNameId = 0;
                return false;
            }

            string[] strArr = urlPath.Split('/');
            string strFileNameId = strArr[strArr.Length - 1];
            bool tryResult = long.TryParse(strFileNameId, out fileNameId);
            return tryResult;
        }

        public static List<LibraryItemX> GetLibIdGroupsList(string libIdGroups)
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

            List<LibraryItemX> libraries = new List<LibraryItemX>();

            foreach (var item in libIdGroupsArray)
            { 
                if(int.TryParse(item, out int libId))
                {
                    var library = businessContext.FtLibrary.Find(libId);
                    if (library != null)
                    {
                        LibraryItemX libraryItemX = new LibraryItemX
                        {
                            Id = library.Id,
                            LibId = library.LibId,
                            Name = library.Name
                        };
                        libraries.Add(libraryItemX);
                    }   
                }
            }

            return libraries;
        }

        /// <summary>
        /// 檢查卡號是否被其他人員佔用 | 沒有佔用 = false
        /// </summary>
        /// <param name="maincomId"></param>
        /// <param name="personId">無id = 0</param>
        /// <param name="cardNo"></param> 
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool CheckPersonCardNoOccupied(string maincomId,long personId, string cardNo, ref ResponseModalX responseModalX)
        {
          
            if (string.IsNullOrEmpty(cardNo))  //排除空白的檢測
                return false;
             
            responseModalX = new ResponseModalX
            {
                meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_CARD_NUMBER_NOT_OCCUPIED, Message = Lang.PERSON_CARD_NUMBER_NOT_OCCUPIED },
                data = null
            };
             
            using BusinessContext businessContext = new BusinessContext();

            FtPerson ftPerson = new FtPerson();

            if (personId==0)
            {
                ftPerson = businessContext.FtPerson.Where(c => c.CardNo.Contains(cardNo) && c.MaincomId.Contains(maincomId)).FirstOrDefault(); 
            }
            else
            {
                ftPerson = businessContext.FtPerson.Where(c => c.CardNo.Contains(cardNo) && c.Id != personId && c.MaincomId.Contains(maincomId)).FirstOrDefault();
            }
           
            if (ftPerson != null)
            {
                responseModalX = new ResponseModalX();
                responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)PersonErrorCode.PERSON_CARD_NUMBER_OCCUPIED, Message = $"{Lang.PERSON_CARD_NUMBER_OCCUPIED} {ftPerson.Name} {ftPerson.Id}" };
                return true;
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_CARD_NUMBER_NOT_OCCUPIED, Message = Lang.PERSON_CARD_NUMBER_NOT_OCCUPIED },
                    data = null
                };
                return false;
            }
        }

        public static Person GetPersonDetails(long personId,out ResponseModalX responseModalX)
        {
            responseModalX = new ResponseModalX();
            using BusinessContext businessContext = new BusinessContext();
            FtPerson ftPerson = businessContext.FtPerson.Find(personId); 
            if (ftPerson == null)
            {
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Success = false, Message = Lang.GeneralUI_NoRecord };
                return null;
            } 

            bool chkPicId = ChkValidOfPersonPicture(ftPerson.Id, out FtPicture ftPicture, out responseModalX);

            List<LibraryItemX> libraryItemXs = new List<LibraryItemX>();

            Person person = new Person
            {
                MaincomId = ftPerson.MaincomId,
                OuterId = ftPerson.OuterId,
                LibId = ftPerson.LibId,
                LibName = ftPerson.Name,
                LibIdGroups = ftPerson.LibIdGroups,
                LibIdGroupsList =string.IsNullOrEmpty(ftPerson.LibIdGroups)? libraryItemXs : GetLibIdGroupsList(ftPerson.LibIdGroups).Select(s => new LibraryItemX { Id = s.Id, LibId = s.LibId, Name = s.Name }).ToList(),
                PersonId = ftPerson.Id,
                Name = ftPerson.Name,
                Sex = ftPerson.Sex.GetValueOrDefault(),
                CardNo = ftPerson.CardNo,
                Phone = ftPerson.Phone,
                Category = ftPerson.Category, 
                Remark = ftPerson.Remark,
                CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", ftPerson.CreateTime)
            };
            if(chkPicId)
            {
                person.PicUrl = ftPicture.PicUrl;
                person.PicClientUrl = ftPicture.PicClientUrl;
            }

            return person;
        }

        public static ResponseModalX GetPersonList(QueryPersonListInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            
            if (string.IsNullOrEmpty(input.MaincomId))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId };
                return responseModalX;
            }
             
            QueryPersonListInfoReturn queryPersonListInfoReturn = new QueryPersonListInfoReturn();
            List<Person> personLists = new List<Person>();

            try
            {
                using (BusinessContext businessContext = new BusinessContext())
                {
                    var ftPersons = businessContext.FtPerson.Where(c => c.Visible == (sbyte)PersonErrorCode.PERSON_IS_VISIBLE && c.MaincomId.Contains(input.MaincomId));
                    
                    if (!string .IsNullOrEmpty(input.Category))
                    {
                        if(Enum.TryParse<PersonCategory>(input.Category.Trim(),out PersonCategory personCategory))
                        {
                            ftPersons = ftPersons.Where(c => c.Category == (sbyte)personCategory);
                        } 
                    }

                    if (!string.IsNullOrEmpty(input.LibraryId))
                    {
                        if (int.TryParse(input.LibraryId.Trim(), out int libId))
                        {
                            ftPersons = ftPersons.Where(c => c.LibId == libId);
                        }
                    }

                    if (!string.IsNullOrEmpty(input.Name))
                    {
                        if(long.TryParse(input.Name,out long personId))
                        {
                            ftPersons = ftPersons.Where(c => c.Id == personId);
                        }
                        else
                        {
                            input.Name = Uri.UnescapeDataString(input.Name).Trim();
                            ftPersons = ftPersons.Where(c => c.Name.Contains(input.Name.Trim()));
                        }
                    }
                    
                    List<LibraryItemX> libraryItemXs = new List<LibraryItemX>();

                    int totalCount = ftPersons.Count();
                    foreach (var item in ftPersons)
                    {
                        bool chkLibId = PersonBusiness.ChkValidOfLibIdExist(item.LibId, out FtLibrary ftLibrary, out responseModalX);
                        bool chkPicId = true;
                        FtPicture ftPicture = new FtPicture(); 
                        if (input.RequiredPic)
                        {
                            chkPicId = PersonBusiness.ChkValidOfPersonPicture(item.Id, out ftPicture, out responseModalX);
                        }
                        //if (chkLibId && chkPicId)   
                        //{
                        //    //取消这两项条件的判断
                        //}
                        //else
                        //{
                        //    continue;
                        //}

                        Person PersonItem = new Person
                        {
                            MaincomId = item.MaincomId,
                            OuterId = item.OuterId,
                            LibId = item.LibId,
                            LibName = ftLibrary?.Name ?? string.Empty,
                            LibIdGroups = item.LibIdGroups,
                            LibIdGroupsList = string.IsNullOrEmpty(item.LibIdGroups) ? libraryItemXs : GetLibIdGroupsList(item.LibIdGroups).Select(s => new LibraryItemX { Id = s.Id, LibId = s.LibId, Name = s.Name ?? string.Empty }).ToList(),
                            PersonId = item.Id,
                            Name = item.Name,
                            Sex = item.Sex.GetValueOrDefault(),
                            CardNo = item.CardNo,
                            Phone = item.Phone,
                            Category = item.Category,
                            PicUrl = ftPicture?.PicUrl ?? string.Empty,
                            PicClientUrl = ftPicture?.PicClientUrl ?? string.Empty,
                            Remark = item.Remark,
                            CreateTime = item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        personLists.Add(PersonItem);
                    }
                    if (personLists.Count == 0)
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.LIST_NO_RECORD, Message = Lang.LIST_NO_RECORD },
                            data = null
                        };
                       return responseModalX;
                    }
                    //返回person list  
                    responseModalX.data = personLists;
                    return responseModalX;
                };
            }
            catch (Exception ex)
            { 
                MetaModalX metaModalX = new MetaModalX { ErrorCode = (int)PersonErrorCode.PERSON_LIST_FAIL, Success = false, Message = $"{Lang.PERSON_LIST_FAIL} [Exception][{ex.Message}]" };
                responseModalX.meta = metaModalX;
                responseModalX.data = null;
                return responseModalX;
            }
        }


    }
}
