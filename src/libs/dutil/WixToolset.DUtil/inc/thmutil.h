#pragma once
// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.


#ifdef __cplusplus
extern "C" {
#endif

// forward declare

typedef struct _THEME_CONTROL THEME_CONTROL;
typedef struct _THEME THEME;

#define ReleaseTheme(p) if (p) { ThemeFree(p); p = NULL; }

typedef HRESULT(CALLBACK *PFNTHM_EVALUATE_VARIABLE_CONDITION)(
    __in_z LPCWSTR wzCondition,
    __out BOOL* pf,
    __in_opt LPVOID pvContext
    );
typedef HRESULT(CALLBACK *PFNTHM_FORMAT_VARIABLE_STRING)(
    __in_z LPCWSTR wzFormat,
    __inout LPWSTR* psczOut,
    __in_opt LPVOID pvContext
    );
typedef HRESULT(CALLBACK *PFNTHM_GET_VARIABLE_NUMERIC)(
    __in_z LPCWSTR wzVariable,
    __out LONGLONG* pllValue,
    __in_opt LPVOID pvContext
    );
typedef HRESULT(CALLBACK *PFNTHM_SET_VARIABLE_NUMERIC)(
    __in_z LPCWSTR wzVariable,
    __in LONGLONG llValue,
    __in_opt LPVOID pvContext
    );
typedef HRESULT(CALLBACK *PFNTHM_GET_VARIABLE_STRING)(
    __in_z LPCWSTR wzVariable,
    __inout LPWSTR* psczValue,
    __in_opt LPVOID pvContext
    );
typedef HRESULT(CALLBACK *PFNTHM_SET_VARIABLE_STRING)(
    __in_z LPCWSTR wzVariable,
    __in_z_opt LPCWSTR wzValue,
    __in BOOL fFormatted,
    __in_opt LPVOID pvContext
    );

typedef enum THEME_ACTION_TYPE
{
    THEME_ACTION_TYPE_BROWSE_DIRECTORY,
    THEME_ACTION_TYPE_CHANGE_PAGE,
    THEME_ACTION_TYPE_CLOSE_WINDOW,
} THEME_ACTION_TYPE;

typedef enum THEME_CONTROL_DATA
{
    THEME_CONTROL_DATA_HOVER = 1,
} THEME_CONTROL_DATA;

typedef enum THEME_CONTROL_TYPE
{
    THEME_CONTROL_TYPE_UNKNOWN,
    THEME_CONTROL_TYPE_BILLBOARD,
    THEME_CONTROL_TYPE_BUTTON,
    THEME_CONTROL_TYPE_CHECKBOX,
    THEME_CONTROL_TYPE_COMBOBOX,
    THEME_CONTROL_TYPE_COMMANDLINK,
    THEME_CONTROL_TYPE_EDITBOX,
    THEME_CONTROL_TYPE_HYPERLINK,
    THEME_CONTROL_TYPE_HYPERTEXT,
    THEME_CONTROL_TYPE_IMAGE,
    THEME_CONTROL_TYPE_LABEL,
    THEME_CONTROL_TYPE_PANEL,
    THEME_CONTROL_TYPE_PROGRESSBAR,
    THEME_CONTROL_TYPE_RADIOBUTTON,
    THEME_CONTROL_TYPE_RICHEDIT,
    THEME_CONTROL_TYPE_STATIC,
    THEME_CONTROL_TYPE_LISTVIEW,
    THEME_CONTROL_TYPE_TREEVIEW,
    THEME_CONTROL_TYPE_TAB,
} THEME_CONTROL_TYPE;

typedef enum THEME_IMAGE_REFERENCE_TYPE
{
    THEME_IMAGE_REFERENCE_TYPE_NONE,
    THEME_IMAGE_REFERENCE_TYPE_PARTIAL,
    THEME_IMAGE_REFERENCE_TYPE_COMPLETE,
} THEME_IMAGE_REFERENCE_TYPE;

typedef enum THEME_SHOW_PAGE_REASON
{
    THEME_SHOW_PAGE_REASON_DEFAULT,
    THEME_SHOW_PAGE_REASON_CANCEL,
    THEME_SHOW_PAGE_REASON_REFRESH,
} THEME_SHOW_PAGE_REASON;

typedef enum THEME_WINDOW_INITIAL_POSITION
{
    THEME_WINDOW_INITIAL_POSITION_DEFAULT,
    THEME_WINDOW_INITIAL_POSITION_CENTER_MONITOR_FROM_COORDINATES,
} THEME_WINDOW_INITIAL_POSITION;

// These messages are sent by thmutil to the parent window created in ThemeCreateParentWindow.
// thmutil reserves the last values of WM_USER's range.
typedef enum _WM_THMUTIL
{
    // Sent while creating a control.
    // wparam is THEME_LOADINGCONTROL_ARGS* and lparam is THEME_LOADINGCONTROL_RESULTS*.
    // Return code is TRUE if it was processed.
    WM_THMUTIL_LOADING_CONTROL = WM_APP - 1,
    // Sent when WM_COMMAND is received for a control.
    // wparam is THEME_CONTROLWMCOMMAND_ARGS* and lparam is THEME_CONTROLWMCOMMAND_RESULTS*.
    // Return code is TRUE if it was processed.
    WM_THMUTIL_CONTROL_WM_COMMAND = WM_APP - 2,
    // Sent when WM_NOTIFY is received for a control.
    // wparam is THEME_CONTROLWMNOTIFY_ARGS* and lparam is THEME_CONTROLWMNOTIFY_RESULTS*.
    // Return code is TRUE to prevent further processing of the message.
    WM_THMUTIL_CONTROL_WM_NOTIFY = WM_APP - 3,
    // Sent after created a control.
    // wparam is THEME_LOADEDCONTROL_ARGS* and lparam is THEME_LOADEDCONTROL_RESULTS*.
    // Return code is TRUE if it was processed.
    WM_THMUTIL_LOADED_CONTROL = WM_APP - 4,
} WM_THMUTIL;

struct THEME_COLUMN
{
    LPWSTR pszName;
    UINT uStringId;
    int nDefaultDpiBaseWidth;
    int nBaseWidth;
    int nWidth;
    BOOL fExpands;
};


struct THEME_IMAGE_REFERENCE
{
    THEME_IMAGE_REFERENCE_TYPE type;
    DWORD dwImageIndex;
    DWORD dwImageInstanceIndex;
    int nX;
    int nY;
    int nHeight;
    int nWidth;
};

struct THEME_IMAGE_INSTANCE
{
    Gdiplus::Bitmap* pBitmap;
};

struct THEME_IMAGE
{
    LPWSTR sczId;
    DWORD dwIndex;

    DWORD cImageInstances;
    THEME_IMAGE_INSTANCE* rgImageInstances;
};


struct THEME_TAB
{
    LPWSTR pszName;
    UINT uStringId;
};

struct THEME_ACTION
{
    LPWSTR sczCondition;
    THEME_ACTION_TYPE type;
    union
    {
        struct
        {
            LPWSTR sczVariableName;
        } BrowseDirectory;
        struct
        {
            LPWSTR sczPageName;
            BOOL fCancel;
        } ChangePage;
    };
};

struct THEME_CONDITIONAL_TEXT
{
    LPWSTR sczCondition;
    LPWSTR sczText;
};

// THEME_ASSIGN_CONTROL_ID - Used to apply a specific id to a named control (usually
//                           to set the WM_COMMAND).
struct THEME_ASSIGN_CONTROL_ID
{
    WORD wId;       // id to apply to control
    LPCWSTR wzName; // name of control to match
    const THEME_CONTROL** ppControl;
    BOOL fDisableAutomaticFunctionality; // prevent declarative functionality from interfering with the application's imperative code
};

const WORD THEME_FIRST_ASSIGN_CONTROL_ID = 0x4000; // Recommended first control id to be assigned.

typedef struct _THEME_CONTROL
{
    THEME_CONTROL_TYPE type;

    WORD wId;
    WORD wPageId;

    LPWSTR sczName; // optional name for control, used to apply control id and link the control to a variable.
    LPWSTR sczText;
    LPWSTR sczTooltip;
    LPWSTR sczNote; // optional text for command link
    int nDefaultDpiX;
    int nDefaultDpiY;
    int nDefaultDpiHeight;
    int nDefaultDpiWidth;
    int nX;
    int nY;
    int nHeight;
    int nWidth;
    UINT uStringId;

    LPWSTR sczEnableCondition;
    LPWSTR sczVisibleCondition;
    BOOL fDisableAutomaticFunctionality;

    union
    {
        struct
        {
            THEME_IMAGE_REFERENCE rgImageRef[4];
        } Button;
        struct
        {
            HBITMAP hImage;
            HICON hIcon;

            DWORD cConditionalNotes;
            THEME_CONDITIONAL_TEXT* rgConditionalNotes;
        } CommandLink;
        struct
        {
            THEME_IMAGE_REFERENCE imageRef;
        } Image;
        struct
        {
            // Don't free these; it's just a handle to the central image lists stored in THEME. The handle is freed once, there.
            HIMAGELIST rghImageList[4];

            THEME_COLUMN* ptcColumns;
            DWORD cColumns;
        } ListView;
        struct
        {
            DWORD cImageRef;
            THEME_IMAGE_REFERENCE* rgImageRef;
        } ProgressBar;
    };

    DWORD dwStyle;
    DWORD dwExtendedStyle;
    DWORD dwInternalStyle;

    DWORD dwFontId;

    // child controls
    DWORD cControls;
    THEME_CONTROL* rgControls;

    // Used by billboard controls
    WORD wBillboardInterval;
    BOOL fBillboardLoops;

    // Used by button and command link controls
    THEME_ACTION* rgActions;
    DWORD cActions;
    THEME_ACTION* pDefaultAction;

    // Used by hyperlink and owner-drawn button controls
    DWORD dwFontHoverId;
    DWORD dwFontSelectedId;

    // Used by radio button controls
    BOOL fLastRadioButton;
    LPWSTR sczValue;
    LPWSTR sczVariable;

    // Used by tab controls
    THEME_TAB *pttTabs;
    DWORD cTabs;

    // Used by controls that have text
    DWORD cConditionalText;
    THEME_CONDITIONAL_TEXT* rgConditionalText;

    // state variables that should be ignored
    HWND hWnd;
    DWORD dwData; // type specific data
    THEME* pTheme;
} THEME_CONTROL;


struct THEME_IMAGELIST
{
    LPWSTR sczName;

    HIMAGELIST hImageList;
};

struct THEME_SAVEDVARIABLE
{
    LPWSTR wzName;
    LPWSTR sczValue;
};

struct THEME_PAGE
{
    WORD wId;
    LPWSTR sczName;

    DWORD cControlIndices;

    DWORD cSavedVariables;
    THEME_SAVEDVARIABLE* rgSavedVariables;
};

struct THEME_FONT_INSTANCE
{
    UINT nDpi;
    HFONT hFont;
};

struct THEME_FONT
{
    LPWSTR sczId;
    DWORD dwIndex;
    LONG lfHeight;
    LONG lfWeight;
    BYTE lfUnderline;
    BYTE lfQuality;
    LPWSTR sczFaceName;

    COLORREF crForeground;
    HBRUSH hForeground;
    COLORREF crBackground;
    HBRUSH hBackground;

    DWORD cFontInstances;
    THEME_FONT_INSTANCE* rgFontInstances;
};


typedef struct _THEME
{
    WORD wNextControlId;

    BOOL fAutoResize;
    BOOL fForceResize;

    DWORD dwStyle;
    DWORD dwFontId;
    HANDLE hIcon;
    LPWSTR sczCaption;
    int nDefaultDpiHeight;
    int nDefaultDpiMinimumHeight;
    int nDefaultDpiWidth;
    int nDefaultDpiMinimumWidth;
    int nHeight;
    int nMinimumHeight;
    int nWidth;
    int nMinimumWidth;
    int nWindowHeight;
    int nWindowWidth;
    UINT uStringId;

    DWORD dwSourceImageInstanceIndex;
    THEME_IMAGE_REFERENCE windowImageRef;

    DWORD cFonts;
    THEME_FONT* rgFonts;

    DWORD cImages;
    THEME_IMAGE* rgImages;

    DWORD cStandaloneImages;
    THEME_IMAGE_INSTANCE* rgStandaloneImages;

    DWORD cPages;
    THEME_PAGE* rgPages;

    DWORD cImageLists;
    THEME_IMAGELIST* rgImageLists;

    DWORD cControls;
    THEME_CONTROL* rgControls;

    // internal state variables -- do not use outside ThmUtil.cpp
    STRINGDICT_HANDLE sdhFontDictionary;
    STRINGDICT_HANDLE sdhImageDictionary;
    HWND hwndParent; // parent for loaded controls
    HWND hwndHover; // current hwnd hovered over
    DWORD dwCurrentPageId;
    HWND hwndTooltip;

    UINT nDpi;

    // callback functions
    PFNTHM_EVALUATE_VARIABLE_CONDITION pfnEvaluateCondition;
    PFNTHM_FORMAT_VARIABLE_STRING pfnFormatString;
    PFNTHM_GET_VARIABLE_NUMERIC pfnGetNumericVariable;
    PFNTHM_SET_VARIABLE_NUMERIC pfnSetNumericVariable;
    PFNTHM_GET_VARIABLE_STRING pfnGetStringVariable;
    PFNTHM_SET_VARIABLE_STRING pfnSetStringVariable;

    LPVOID pvVariableContext;
} THEME;

typedef struct _THEME_CONTROLWMCOMMAND_ARGS
{
    DWORD cbSize;
    WPARAM wParam;
    const THEME_CONTROL* pThemeControl;
} THEME_CONTROLWMCOMMAND_ARGS;

typedef struct _THEME_CONTROLWMCOMMAND_RESULTS
{
    DWORD cbSize;
    LRESULT lResult;
} THEME_CONTROLWMCOMMAND_RESULTS;

typedef struct _THEME_CONTROLWMNOTIFY_ARGS
{
    DWORD cbSize;
    LPNMHDR lParam;
    const THEME_CONTROL* pThemeControl;
} THEME_CONTROLWMNOTIFY_ARGS;

typedef struct _THEME_CONTROLWMNOTIFY_RESULTS
{
    DWORD cbSize;
    LRESULT lResult;
} THEME_CONTROLWMNOTIFY_RESULTS;

typedef struct _THEME_LOADEDCONTROL_ARGS
{
    DWORD cbSize;
    const THEME_CONTROL* pThemeControl;
} THEME_LOADEDCONTROL_ARGS;

typedef struct _THEME_LOADEDCONTROL_RESULTS
{
    DWORD cbSize;
    HRESULT hr;
} THEME_LOADEDCONTROL_RESULTS;

typedef struct _THEME_LOADINGCONTROL_ARGS
{
    DWORD cbSize;
    const THEME_CONTROL* pThemeControl;
} THEME_LOADINGCONTROL_ARGS;

typedef struct _THEME_LOADINGCONTROL_RESULTS
{
    DWORD cbSize;
    HRESULT hr;

    // Used to apply a specific id to the control (usually used for WM_COMMAND).
    // If assigning an id, it is recommended to start with THEME_FIRST_ASSIGN_CONTROL_ID to avoid collisions with well known ids such as IDOK and IDCANCEL.
    // The values [100, THEME_FIRST_ASSIGN_CONTROL_ID) are reserved for thmutil.
    // Due to this value being packed into 16 bits for many system window messages, this is restricted to a WORD.
    WORD wId;
    // Used to prevent declarative functionality from interfering with the application's imperative code.
    BOOL fDisableAutomaticFunctionality;
} THEME_LOADINGCONTROL_RESULTS;


/********************************************************************
 ThemeInitialize - initialized theme management.

*******************************************************************/
HRESULT DAPI ThemeInitialize(
    __in_opt HMODULE hModule
    );

/********************************************************************
 ThemeUninitialize - uninitialize theme management.

*******************************************************************/
void DAPI ThemeUninitialize();

/********************************************************************
 ThemeLoadFromFile - loads a theme from a loose file.

 *******************************************************************/
HRESULT DAPI ThemeLoadFromFile(
    __in_z LPCWSTR wzThemeFile,
    __out THEME** ppTheme
    );

/********************************************************************
 ThemeLoadFromResource - loads a theme from a module's data resource.

 NOTE: The resource data must be UTF-8 encoded.
*******************************************************************/
HRESULT DAPI ThemeLoadFromResource(
    __in_opt HMODULE hModule,
    __in_z LPCSTR szResource,
    __out THEME** ppTheme
    );

/********************************************************************
 ThemeFree - frees any memory associated with a theme.

*******************************************************************/
void DAPI ThemeFree(
    __in THEME* pTheme
    );

/********************************************************************
ThemeRegisterVariableCallbacks - registers a context and callbacks
                                 for working with variables.

*******************************************************************/
HRESULT DAPI ThemeRegisterVariableCallbacks(
    __in THEME* pTheme,
    __in_opt PFNTHM_EVALUATE_VARIABLE_CONDITION pfnEvaluateCondition,
    __in_opt PFNTHM_FORMAT_VARIABLE_STRING pfnFormatString,
    __in_opt PFNTHM_GET_VARIABLE_NUMERIC pfnGetNumericVariable,
    __in_opt PFNTHM_SET_VARIABLE_NUMERIC pfnSetNumericVariable,
    __in_opt PFNTHM_GET_VARIABLE_STRING pfnGetStringVariable,
    __in_opt PFNTHM_SET_VARIABLE_STRING pfnSetStringVariable,
    __in_opt LPVOID pvContext
    );

/********************************************************************
 ThemeInitializeWindowClass - sets defaults for the window class
                              from the given theme.

*******************************************************************/
void DAPI ThemeInitializeWindowClass(
    __in THEME* pTheme,
    __in WNDCLASSW* pWndClass,
    __in WNDPROC pfnWndProc,
    __in HINSTANCE hInstance,
    __in LPCWSTR wzClassName
    );

/********************************************************************
 ThemeCreateParentWindow - creates a parent window for the theme.

*******************************************************************/
HRESULT DAPI ThemeCreateParentWindow(
    __in THEME* pTheme,
    __in DWORD dwExStyle,
    __in LPCWSTR szClassName,
    __in LPCWSTR szWindowName,
    __in DWORD dwStyle,
    __in int x,
    __in int y,
    __in_opt HWND hwndParent,
    __in_opt HINSTANCE hInstance,
    __in_opt LPVOID lpParam,
    __in THEME_WINDOW_INITIAL_POSITION initialPosition,
    __out_opt HWND* phWnd
    );

/********************************************************************
 ThemeLocalize - Localizes all of the strings in the theme.

*******************************************************************/
HRESULT DAPI ThemeLocalize(
    __in THEME *pTheme,
    __in const WIX_LOCALIZATION *pLocStringSet
    );

HRESULT DAPI ThemeLoadStrings(
    __in THEME* pTheme,
    __in HMODULE hResModule
    );

/********************************************************************
 ThemeLoadRichEditFromFile - Attach a richedit control to a RTF file.

 *******************************************************************/
HRESULT DAPI ThemeLoadRichEditFromFile(
    __in const THEME_CONTROL* pThemeControl,
    __in_z LPCWSTR wzFileName,
    __in HMODULE hModule
    );

/********************************************************************
 ThemeLoadRichEditFromResource - Attach a richedit control to resource data.

 *******************************************************************/
HRESULT DAPI ThemeLoadRichEditFromResource(
    __in const THEME_CONTROL* pThemeControl,
    __in_z LPCSTR szResourceName,
    __in HMODULE hModule
    );

/********************************************************************
 ThemeHandleKeyboardMessage - will translate the message using the active
                             accelerator table.

*******************************************************************/
BOOL DAPI ThemeHandleKeyboardMessage(
    __in_opt THEME* pTheme,
    __in HWND hWnd,
    __in MSG* pMsg
    );

/********************************************************************
 ThemeDefWindowProc - replacement for DefWindowProc() when using theme.

*******************************************************************/
LRESULT CALLBACK ThemeDefWindowProc(
    __in_opt THEME* pTheme,
    __in HWND hWnd,
    __in UINT uMsg,
    __in WPARAM wParam,
    __in LPARAM lParam
    );

/********************************************************************
 ThemeGetPageIds - gets the page ids for the theme via page names.

*******************************************************************/
void DAPI ThemeGetPageIds(
    __in const THEME* pTheme,
    __in_ecount(cGetPages) LPCWSTR* rgwzFindNames,
    __inout_ecount(cGetPages) DWORD* rgdwPageIds,
    __in DWORD cGetPages
    );

/********************************************************************
 ThemeGetPage - gets a theme page by id.

 *******************************************************************/
THEME_PAGE* DAPI ThemeGetPage(
    __in const THEME* pTheme,
    __in DWORD dwPage
    );

/********************************************************************
 ThemeShowPage - shows or hides all of the controls in the page at one time.

 *******************************************************************/
HRESULT DAPI ThemeShowPage(
    __in THEME* pTheme,
    __in DWORD dwPage,
    __in int nCmdShow
    );

/********************************************************************
ThemeShowPageEx - shows or hides all of the controls in the page at one time.
                  When using variables, TSPR_CANCEL reverts any changes made.
                  TSPR_REFRESH forces reevaluation of conditions.
                  It is expected that the current page is hidden before 
                  showing a new page.

*******************************************************************/
HRESULT DAPI ThemeShowPageEx(
    __in THEME* pTheme,
    __in DWORD dwPage,
    __in int nCmdShow,
    __in THEME_SHOW_PAGE_REASON reason
    );


/********************************************************************
ThemeShowChild - shows a control's specified child control, hiding the rest.

*******************************************************************/
void DAPI ThemeShowChild(
    __in THEME_CONTROL* pParentControl,
    __in DWORD dwIndex
    );

/********************************************************************
 ThemeControlExistsByHwnd - check if a control with the specified hWnd exists.

 *******************************************************************/
BOOL DAPI ThemeControlExistsByHWnd(
    __in const THEME* pTheme,
    __in HWND hWnd,
    __out_opt const THEME_CONTROL** ppThemeControl
    );

/********************************************************************
 ThemeControlExistsById - check if a control with the specified id exists.

 *******************************************************************/
BOOL DAPI ThemeControlExistsById(
    __in const THEME* pTheme,
    __in WORD wId,
    __out_opt const THEME_CONTROL** ppThemeControl
    );

/********************************************************************
 ThemeControlEnable - enables/disables a control.

 *******************************************************************/
void DAPI ThemeControlEnable(
    __in const THEME_CONTROL* pThemeControl,
    __in BOOL fEnable
    );

/********************************************************************
 ThemeControlEnabled - returns whether a control is enabled/disabled.

 *******************************************************************/
BOOL DAPI ThemeControlEnabled(
    __in const THEME_CONTROL* pThemeControl
    );

/********************************************************************
 ThemeControlElevates - sets/removes the shield icon on a control.

 *******************************************************************/
void DAPI ThemeControlElevates(
    __in const THEME_CONTROL* pThemeControl,
    __in BOOL fElevates
    );

/********************************************************************
 ThemeShowControl - shows/hides a control.

 *******************************************************************/
void DAPI ThemeShowControl(
    __in const THEME_CONTROL* pThemeControl,
    __in int nCmdShow
    );

/********************************************************************
ThemeShowControlEx - shows/hides a control with support for 
conditional text and notes.

*******************************************************************/
void DAPI ThemeShowControlEx(
    __in const THEME_CONTROL* pThemeControl,
    __in int nCmdShow
    );

/********************************************************************
 ThemeControlVisible - returns whether a control is visible.

 *******************************************************************/
BOOL DAPI ThemeControlVisible(
    __in const THEME_CONTROL* pThemeControl
    );

/********************************************************************
 ThemeDrawBackground - draws the theme background.

*******************************************************************/
HRESULT DAPI ThemeDrawBackground(
    __in THEME* pTheme,
    __in PAINTSTRUCT* pps
    );

/********************************************************************
 ThemeDrawControl - draw an owner drawn control.

*******************************************************************/
HRESULT DAPI ThemeDrawControl(
    __in THEME* pTheme,
    __in DRAWITEMSTRUCT* pdis
    );

/********************************************************************
 ThemeIsControlChecked - gets whether a control is checked. Only
                         really useful for checkbox controls.

*******************************************************************/
BOOL DAPI ThemeIsControlChecked(
    __in const THEME_CONTROL* pThemeControl
    );

/********************************************************************
 ThemeSetProgressControl - sets the current percentage complete in a
                           progress bar control.

*******************************************************************/
HRESULT DAPI ThemeSetProgressControl(
    __in const THEME_CONTROL* pThemeControl,
    __in DWORD dwProgressPercentage
    );

/********************************************************************
 ThemeSetProgressControlColor - sets the current color of a
                                progress bar control.

*******************************************************************/
HRESULT DAPI ThemeSetProgressControlColor(
    __in const THEME_CONTROL* pThemeControl,
    __in DWORD dwColorIndex
    );

/********************************************************************
 ThemeSetTextControl - sets the text of a control.

*******************************************************************/
HRESULT DAPI ThemeSetTextControl(
    __in const THEME_CONTROL* pThemeControl,
    __in_z_opt LPCWSTR wzText
    );

/********************************************************************
ThemeSetTextControl - sets the text of a control and optionally
                      invalidates the control.

*******************************************************************/
HRESULT DAPI ThemeSetTextControlEx(
    __in const THEME_CONTROL* pThemeControl,
    __in BOOL fUpdate,
    __in_z_opt LPCWSTR wzText
    );

/********************************************************************
 ThemeGetTextControl - gets the text of a control.

*******************************************************************/
HRESULT DAPI ThemeGetTextControl(
    __in const THEME_CONTROL* pThemeControl,
    __inout_z LPWSTR* psczText
    );

/********************************************************************
 ThemeUpdateCaption - updates the caption in the theme.

*******************************************************************/
HRESULT DAPI ThemeUpdateCaption(
    __in THEME* pTheme,
    __in_z LPCWSTR wzCaption
    );

/********************************************************************
 ThemeSetFocus - set the focus to the control supplied or the next 
                 enabled control if it is disabled.

*******************************************************************/
void DAPI ThemeSetFocus(
    __in const THEME_CONTROL* pThemeControl
    );

#ifdef __cplusplus
}
#endif

