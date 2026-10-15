using System;

namespace RD.Draw
{
    /// <summary>
    /// 图标类型（Bootstrap Icons 风格）。
    /// 编辑器侧负责把图标解析为可绘制纹理。
    /// </summary>
    public enum SdfIconType
    {
        None = 0,
        // 导航
        ArrowLeft, ArrowRight, ArrowUp, ArrowDown,
        ArrowLeftShort, ArrowRightShort, ArrowUpShort, ArrowDownShort,
        ArrowLeftCircle, ArrowRightCircle, ArrowUpCircle, ArrowDownCircle,
        ArrowLeftCircleFill, ArrowRightCircleFill, ArrowUpCircleFill, ArrowDownCircleFill,
        ArrowRepeat, ArrowRepeatFill, ArrowClockwise, ArrowCounterclockwise,
        ChevronLeft, ChevronRight, ChevronUp, ChevronDown,
        ChevronLeftFill, ChevronRightFill, ChevronUpFill, ChevronDownFill,
        CaretLeft, CaretRight, CaretUp, CaretDown,
        CaretLeftFill, CaretRightFill, CaretUpFill, CaretDownFill,
        CornerDownLeft, CornerDownRight, CornerUpLeft, CornerUpRight,
        // 购物车 / 购买
        Cart, CartFill, CartPlus, CartPlusFill, CartCheck, CartDash, CartX,
        Cart2, Cart3, Cart4, Bag, BagFill, BagCheck, BagDash, BagPlus, BagX,
        CreditCard, CreditCard2FrontFill, Bank, Bank2, Wallet, WalletFill, Wallet2,
        Cash, CashCoin, CashStack, CurrencyDollar, CurrencyEuro, CurrencyPound, CurrencyYen,
        // 人员
        Person, PersonFill, PersonPlus, PersonPlusFill, PersonCheck, PersonX,
        PersonBadge, PersonCircle, PersonLinesFill, PersonSquare,
        People, PeopleFill, PersonVcard, PersonVcardFill,
        // 搜索 / 缩放
        Search, SearchHeart, SearchHeartFill, ZoomIn, ZoomOut,
        // 运算符号
        Plus, PlusCircle, PlusCircleFill, PlusSquare, PlusSquareFill, PlusLg,
        Minus, MinusCircle, MinusCircleFill, MinusSquare, MinusSquareFill, Dash, DashCircle, DashCircleFill,
        X, XCircle, XCircleFill, XSquare, XSquareFill, XLg,
        Check, CheckCircle, CheckCircleFill, CheckSquare, CheckSquareFill, Check2, Check2Circle, CheckLg,
        Divide, Percent, Asterisk,
        // 连接 / 外链
        Link, Link45deg, LinkBreak, LinkBreakFill, Link2, Link2Fill, Unlink,
        BoxArrowUpRight, BoxArrowDownRight, BoxArrowInUpRight, BoxArrowInDownRight,
        // 代码仓库 / 版本控制
        Github, Git, GitBranch, GitCommit, GitMerge, GitPull, GitFork, Gitlab,
        Terminal, TerminalFill, Code, CodeSquare, CodeSlash, Braces, BracesAsterisk,
        // 设置 / 工具
        Settings, SettingsFill, Gear, GearFill, GearWide, GearWideConnected, Sliders, Sliders2, Sliders2Vertical,
        Toggles, ToggleOff, ToggleOn, Toggle2Off, Toggle2On,
        Wrench, WrenchAdjustable, WrenchAdjustableCircleFill, Hammer, Tools,
        Pencil, PencilFill, PencilSquare, Eraser, Brush, BrushFill,
        // 文件 / 文件夹
        FileText, FileTextFill, FileEarmarkText, FileEarmarkTextFill, FileEarmark, FileEarmarkFill,
        FileEarmarkPlus, FilePlus, FileMinus, FileX, FileCheck, FileCode, FileImage, FileMusic, FilePlay,
        Folder, FolderFill, Folder2, Folder2Open, FolderPlus, FolderMinus, FolderX, FolderCheck,
        FolderOpen, FolderSymlink, Inbox, InboxFill, Archive, ArchiveFill,
        // 保存 / 传输
        Save, Save2, Save2Fill, Download, DownloadFill, Upload, UploadFill,
        CloudDownload, CloudUpload, CloudArrowDown, CloudArrowUp, CloudCheck,
        ArrowDownUp, ArrowUpDown, ArrowLeftRight, ArrowBarUp, ArrowBarDown,
        // 常用交互
        House, HouseFill, DoorOpen, DoorClosed, Eye, EyeFill, EyeSlash, EyeSlashFill,
        Book, BookFill, Bookmark, BookmarkFill, BookmarkStar, BookmarkHeart,
        Bell, BellFill, BellSlash, BellSlashFill, Clock, ClockFill, ClockHistory, Hourglass,
        Calendar, Calendar2, CalendarEvent, CalendarEventFill, CalendarX, CalendarCheck, CalendarPlus, Alarm, AlarmFill,
        Refresh, Sync, Repeat, Recycle, RecycleFill,
        // 编辑排版
        Type, TypeBold, TypeItalic, TypeUnderline, TypeStrikethrough, TextLeft, TextRight, TextCenter, TextJustify,
        AlignLeft, AlignRight, AlignCenter, AlignJustify, List, ListUl, ListOl, ListCheck,
        Grid, GridFill, Grid3X3Gap, Grid3X3GapFill, Columns, ColumnsGap, Rows, LayoutSidebar, LayoutSidebarReverse,
        // 通信
        Chat, ChatFill, ChatLeft, ChatLeftFill, ChatRight, ChatRightFill, ChatQuote, ChatQuoteFill,
        Envelope, EnvelopeFill, EnvelopeOpen, EnvelopePaper, Send, SendFill, SendExclamation, Telephone,
        Globe, Globe2, Compass, CompassFill, Geo, GeoAlt, GeoAltFill, Map, MapFill, MapMarker, MapMarkerFill,
        // 状态
        Info, InfoCircle, InfoCircleFill, Question, QuestionCircle, QuestionCircleFill, QuestionLg,
        Exclamation, ExclamationCircle, ExclamationCircleFill, ExclamationTriangle, ExclamationTriangleFill,
        Lightbulb, LightbulbFill, Lightning, LightningFill, Fire, FireFill, Shield, ShieldFill, ShieldLock,
        ShieldCheck, ShieldExclamation, Umbrella, Award, AwardFill, Trophy, TrophyFill, Medal, PatchCheck,
        PatchCheckFill, PatchExclamation, Star, StarFill, StarHalf, Heart, HeartFill, HeartHalf, HeartPulse,
        ThumbsUp, ThumbsUpFill, ThumbsDown, ThumbsDownFill, HandThumbsUp, HandThumbsUpFill,
        // 媒体
        Play, PlayFill, PlayCircle, PlayCircleFill, Pause, PauseFill, Stop, StopFill,
        FastForward, FastForwardFill, Rewind, RewindFill, SkipForward, SkipBackward, SkipForwardFill, SkipBackwardFill,
        Record, RecordFill, RecordCircle, RecordCircleFill, MusicNote, MusicNoteBeamed, MusicNoteList,
        Camera, CameraFill, CameraVideo, CameraVideoFill, Film, Image, ImageFill, Images,
        Mic, MicFill, MicMute, Speaker, SpeakerFill, VolumeUp, VolumeUpFill, VolumeDown, VolumeMute,
        // 常用业务
        Building, BuildingFill, Briefcase, BriefcaseFill, Clipboard, ClipboardFill, ClipboardCheck,
        Trash, TrashFill, Trash2, Trash2Fill, Trash3, Trash3Fill,
        Filter, Funnel, FunnelFill, SortUp, SortDown, SortAlphaDown,
        Palette, PaintBucket, PaintBucketFill, Bucket, Bricks, Layers, LayersFill, LayerBackward, LayerForward,
        Box, BoxFill, BoxSeam, Box2, Boxes, Cube, CubeFill,
        Key, KeyFill, Lock, LockFill, Unlock, UnlockFill, Fingerprint,
        Egg, EggFill, EggFried, Cup, CupFill, CupHot, CupStraw,
        ThermometerHalf, ThermometerSun, Droplet, DropletFill, DropletHalf, Water,
        Cloud, CloudFill, CloudRain, CloudRainFill, CloudSun, CloudSunFill, Moon, MoonFill, MoonStars,
        Sun, SunFill, Sunrise, Sunset, Stars, Sparkles,
        Gem, Diamond, GemFill, Puzzle, PuzzleFill, PuzzlePiece,
        Justify, JustifyLeft, JustifyRight, ThreeDots, ThreeDotsVertical, ThreeDotsFill, MenuButton,
        MenuButtonWideFill, MenuUp, MenuDown, App, AppIndicator, Square, SquareFill, Circle, CircleFill,
    }
}
