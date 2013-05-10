/* Copyright (c) <2009> <Newton Game Dynamics>
* 
* This software is provided 'as-is', without any express or implied
* warranty. In no event will the authors be held liable for any damages
* arising from the use of this software.
* 
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications, and to alter it and redistribute it
* freely
*/

// NewtonDemos.cpp : Defines the entry point for the application.
//


#include <toolbox_stdafx.h>
#include "SkyBox.h"
#include "DemoMenu.h"
#include "DemoCamera.h"
#include "NewtonDemos.h"
#include "PhysicsUtils.h"
#include "DebugDisplay.h"
#include "DemoEntityManager.h"


//#define DEFAULT_SCENE	0			// using NetwonMesh Tool
//#define DEFAULT_SCENE	1			// Coefficients of friction
//#define DEFAULT_SCENE	2			// Coefficients of restitution
//#define DEFAULT_SCENE	3			// Precessing tops
//#define DEFAULT_SCENE	4			// closest distance
//#define DEFAULT_SCENE	5			// primitive collision
//#define DEFAULT_SCENE	6 			// Kinematic bodies
//#define DEFAULT_SCENE	7			// primitive convex cast 
//#define DEFAULT_SCENE	8			// Box stacks
//#define DEFAULT_SCENE	9			// simple level mesh collision
//#define DEFAULT_SCENE	10			// optimized level mesh collision
//#define DEFAULT_SCENE	11			// height field Collision
//#define DEFAULT_SCENE	12			// infinite user plane collision
//#define DEFAULT_SCENE	13			// user height field Collision
//#define DEFAULT_SCENE	14			// compound Collision
//#define DEFAULT_SCENE	15			// pjani compound bug
//#define DEFAULT_SCENE	16			// uniform Scaled Collision
//#define DEFAULT_SCENE	17			// non Uniform Scaled Collision
//#define DEFAULT_SCENE	18			// scaled mesh collision
//#define DEFAULT_SCENE	19			// simple convex decomposition
//#define DEFAULT_SCENE	20			// scene Collision
//#define DEFAULT_SCENE	21          // simple boolean operators 
//#define DEFAULT_SCENE	22			// simple convex Shatter
//#define DEFAULT_SCENE	23			// multi ray casting using the threading Job scheduler
//#define DEFAULT_SCENE	24			// continue collision
//#define DEFAULT_SCENE	25			// puck slide continue collision
#define DEFAULT_SCENE	26			// basic car
// #define DEFAULT_SCENE	27			// player controller
//#define DEFAULT_SCENE	28			// advanced player controller
//#define DEFAULT_SCENE	29			// high performance super car
//#define DEFAULT_SCENE	30			// soft bodies			


// Message Map
FXDEFMAP(NewtonDemos) NewtonDemosMessageMap[]=
{
	FXMAPFUNC(SEL_PAINT,		NewtonDemos::ID_CANVAS,								NewtonDemos::onPaint),
	FXMAPFUNC(SEL_CHORE,		NewtonDemos::ID_CHORE,								NewtonDemos::onChore),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_NEW,								NewtonDemos::onNew),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_LOAD,								NewtonDemos::onLoad),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_SAVE,								NewtonDemos::onSave),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_SERIALIZE,							NewtonDemos::onSerializeWorld),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_DESERIALIZE,						NewtonDemos::onDeserializeWorld),

	FXMAPFUNC(SEL_JOYSTICK,		0,													NewtonDemos::onJoysticMove),

	FXMAPFUNC(SEL_KEYPRESS,		0,													NewtonDemos::onKeyPress),
	FXMAPFUNC(SEL_KEYRELEASE,	0,													NewtonDemos::onKeyRelease),
	FXMAPFUNC(SEL_MOTION,		NewtonDemos::ID_CANVAS,								NewtonDemos::onMouseMove),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_HIDE_VISUAL_MESHES,					NewtonDemos::onHideVisualMeshes),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_SHOW_COLLISION_MESH,				NewtonDemos::onShowCollisionLines),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_SHOW_CONTACT_POINTS,				NewtonDemos::onShowContactPoints),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_SHOW_NORMAL_FORCES,					NewtonDemos::onShowNormalForces),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_SHOW_AABB,							NewtonDemos::onShowAABB),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_AUTOSLEEP_MODE,						NewtonDemos::onAutoSleepMode),

	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_USE_PARALLEL_SOLVER,				NewtonDemos::onUseParallelSolver),
	
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_SHOW_CONCURRENCE_PROFILER,			NewtonDemos::onShowConcurrentProfiler),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_SHOW_PROFILER,						NewtonDemos::onShowThreadProfiler),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_SELECT_ALL_PROFILERS,				NewtonDemos::onSelectAllPerformanceChart),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_UNSELECT_ALL_PROFILERS,				NewtonDemos::onUnselectAllPerformanceChart),
	FXMAPFUNC(SEL_COMMAND,		NewtonDemos::ID_CONCURRENT_PHYSICS_UPDATE,			NewtonDemos::onRunPhysicsConcurrent),
	
	FXMAPFUNCS(SEL_COMMAND,		NewtonDemos::ID_SHOW_PHYSICS_PROFILER,	NewtonDemos::ID_SHOW_PHYSICS_PROFILER + 16 ,	NewtonDemos::onShowProfiler),

	FXMAPFUNCS(SEL_COMMAND,		NewtonDemos::ID_PLATFORMS,				NewtonDemos::ID_PLATFORMS + 16,					NewtonDemos::onSimdInstructions),
	FXMAPFUNCS(SEL_COMMAND,		NewtonDemos::ID_DYNAMICS_BROADPHASE,	NewtonDemos::ID_HYBRID_BROADPHASE,				NewtonDemos::onBroadPhaseType),
	FXMAPFUNCS(SEL_COMMAND,		NewtonDemos::ID_SELECT_MICROTHREADS,	NewtonDemos::ID_SELECT_MICROTHREADS + 16,		NewtonDemos::onSelectNumberOfMicroThreads),
	FXMAPFUNCS(SEL_COMMAND,		NewtonDemos::ID_RUN_DEMO,				NewtonDemos::ID_RUN_DEMO_RANGE,					NewtonDemos::onRunDemo),
};


// Implementation
FXIMPLEMENT(NewtonDemos,FXMainWindow,NewtonDemosMessageMap,ARRAYNUMBER(NewtonDemosMessageMap))



#define SCREEN_WIDTH 800
#define SCREEN_HEIGHT 600

int NewtonDemos::m_totalMemoryUsed = 0;


void *operator new (size_t size) 
{ 
	void* const ptr = ::malloc (size);
//	unsigned xxx = unsigned (ptr);
//	xxx &= 0xffff;
//	_ASSERTE (xxx != 0xAB78);
//	_ASSERTE (!((xxx == 0xAB68) && (size >= 2080)));
//	dTrace (("%d %x\n", xxx, ptr))
	return ptr; 
}                                          

void operator delete (void *ptr) 
{ 
	::free (ptr); 
}


int main(int argc, char *argv[])
{
	// Enable run-time memory check for debug builds.
#ifdef _MSC_VER
	_CrtSetDbgFlag( _CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF );
#endif



	FXApp application("Newton Dynamics demos", "Newton Dynamics demos");

	// Set the memory allocation function before creation the newton world
	// this is the only function that can be called before the creation of the newton world.
	// it should be called once, and the the call is optional 
	NewtonSetMemorySystem (NewtonDemos::PhysicsAlloc, NewtonDemos::PhysicsFree);

	application.init(argc,argv);

	// Make main window
	NewtonDemos* const mainWindow = new NewtonDemos (application);

	// Create the application's windows
	application.create();

	// load the default Scene		
	mainWindow->LoadDemo (DEFAULT_SCENE + NewtonDemos::ID_RUN_DEMO);

	// start the real time loop, by sending a message that send himself again
	application.addChore(mainWindow, NewtonDemos::ID_CHORE);
		

//for (int i = 0; i < 1000; i ++)
//	NewtonDestroy(NewtonCreate()); 

	
	// Run the application
	return application.run();
}


NewtonDemos::NewtonDemos()
{
}


NewtonDemos::NewtonDemos(FXApp& application)
	:FXMainWindow(&application, "Newton Dynamics 3.00 unit test demos", NULL, NULL, DECOR_ALL, 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT)
	,m_autoSleepState(false)
	,m_useParallelSolver(false)
//	,m_useParallelSolver(true)
	,m_hideVisualMeshes(false)
	,m_debugDisplayState(false)
//	,m_debugDisplayState(true)
	,m_showAABB(false)
	,m_showContactPoints(false)
	,m_showNormalForces(false)
//	,m_showNormalForces(true)
	,m_physicProfilerState(true)
	,m_threadProfilerState(true)
	,m_concurrentProfilerState(true)
//	,m_showContactPointState(false)
	,m_showStatistics(true)
	,m_doVisualUpdates(true)
	,m_mousePosX(0)
	,m_mousePosY(0)
	,m_joytickX(0)
	,m_joytickY(0)
	,m_joytickButtonMask(0)
	,m_hasJoysticController(false)
	,m_menubar(NULL)
	,m_mainMenu(NULL)
	,m_statusbar(NULL)
	,m_scene(NULL)
	,m_framesCount(0)
	,m_timestepAcc(0)
	,m_fps(0.0f)
	,m_broadPhaseMode (ID_DYNAMICS_BROADPHASE)
	,m_broadPhaseSelection(m_broadPhaseMode)
	,m_cpuInstructionsMode(ID_PLATFORMS)
//	,m_cpuInstructionsMode(ID_USE_AVX_INSTRUCTIONS)
	,m_cpuInstructionSelection (m_cpuInstructionsMode)
	,m_microthreadCount(ID_SELECT_MICROTHREADS)
	,m_microthreadCountSelection(m_microthreadCount)
	,m_physicsUpdateMode(0)
{
	m_broadPhaseSelection.setTarget(this);
	m_broadPhaseSelection.setSelector(ID_DYNAMICS_BROADPHASE);

	m_cpuInstructionSelection.setTarget(this);
	m_cpuInstructionSelection.setSelector(ID_PLATFORMS);

	m_microthreadCountSelection.setTarget(this);
	m_microthreadCountSelection.setSelector(ID_SELECT_MICROTHREADS);


	// create status bar for showing results 
	m_statusbar = new FXStatusBar(this, LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|STATUSBAR_WITH_DRAGCORNER);
	// init the fps status bar
	CalculateFPS(0.0f);

	// create the main menu
	FXDockSite* const dockMenuFrame = new FXDockSite(this,DOCKSITE_NO_WRAP|LAYOUT_SIDE_TOP|LAYOUT_FILL_X);


	// Set the initial states
	onShowCollisionLines(this, 0, (void*) m_debugDisplayState);

	// create the open gl canvas and scene manager
	FXHorizontalFrame* const glFrame = new FXHorizontalFrame(this,LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y, 0,0,0,0, 0,0,0,0, 4,4);

	//create a main frame to hold the Render canvas
	m_scene = new DemoEntityManager (glFrame, this);

	// create main menu
	m_mainMenu = new DemoMenu (dockMenuFrame, this);

	// clear the key map
	memset (m_key, 0, sizeof (m_key));
	for (int i = 0; i < int (sizeof (m_keyMap)/sizeof (m_keyMap[0])); i ++) {
		m_keyMap[i] = i;
	}
	for (int i = 'a'; i <= 'z'; i ++) {
		m_keyMap[i] = i - 'a' + 'A';
	}

#ifdef WIN32
	m_keyMap[0] = VK_LBUTTON;
	m_keyMap[1] = VK_RBUTTON;
	m_keyMap[2] = VK_MBUTTON; 
#endif



	NewtonSelectBroadphaseAlgorithm (m_scene->GetNewton(), m_broadPhaseMode - ID_DYNAMICS_BROADPHASE);
	m_broadPhaseMode = m_broadPhaseMode - FXDataTarget::ID_OPTION;

	//m_mainMenu->m_cpuModes[2]->disable();
	NewtonSetCurrentDevice (m_scene->GetNewton(), m_cpuInstructionsMode - ID_PLATFORMS); 
	m_cpuInstructionsMode = m_cpuInstructionsMode - FXDataTarget::ID_OPTION;


	NewtonSetThreadsCount(m_scene->GetNewton(), m_mainMenu->m_threadsTracks[m_microthreadCount-ID_SELECT_MICROTHREADS]); 
	m_microthreadCount = m_microthreadCount - FXDataTarget::ID_OPTION;

	if (m_useParallelSolver) {
		NewtonSetMultiThreadSolverOnSingleIsland (m_scene->GetNewton(), 1);
	}

	if (m_physicProfilerState) {
		m_scene->m_showProfiler[NEWTON_PROFILER_WORLD_UPDATE] = 1;
	}
}


NewtonDemos::~NewtonDemos()
{
}


void NewtonDemos::BEGIN_MENU_OPTION()																				
{
	m_doVisualUpdates = false;																			
	if (m_scene && m_scene->GetNewton()) {																			
		NewtonWaitForUpdateToFinish (m_scene->GetNewton());												
	}
}


void NewtonDemos::END_MENU_OPTION()
{
	m_doVisualUpdates = true;																			
	if (m_scene && m_scene->GetNewton()) {		
		NewtonWaitForUpdateToFinish (m_scene->GetNewton());
		SetAutoSleepMode (m_scene->GetNewton(), m_autoSleepState);
		NewtonSetMultiThreadSolverOnSingleIsland (m_scene->GetNewton(), m_useParallelSolver ? 1 : 0);	
	}
}


void NewtonDemos::create()
{
	FXMainWindow::create();
	show(PLACEMENT_SCREEN);
}



// memory allocation for Newton
void* NewtonDemos::PhysicsAlloc (int sizeInBytes)
{
	m_totalMemoryUsed += sizeInBytes;
	return new char[sizeInBytes];
}

// memory free use by the engine
void NewtonDemos::PhysicsFree (void* ptr, int sizeInBytes)
{
	m_totalMemoryUsed -= sizeInBytes;
	delete[] (char*)ptr;
}

void NewtonDemos::CalculateFPS(float timestep)
{
	m_framesCount ++;
	m_timestepAcc += timestep;

	// this probably happing on loading of and a pause, just rest counters
	if ((m_timestepAcc <= 0.0f) || (m_timestepAcc > 2.0f)){
		m_timestepAcc = 0;
		m_framesCount = 0;
	}

	//update fps every quarter of a second
	if (m_timestepAcc >= 0.25f) {
		m_fps = float (m_framesCount) / m_timestepAcc;
		m_timestepAcc -= 0.25f;
		m_framesCount = 0.0f;

		char statusText [512] ;
		NewtonWorld* const world = m_scene->GetNewton();
		char platform[256];
		NewtonGetDeviceString(world, NewtonGetCurrentDevice(world), platform, sizeof (platform));
		int memoryUsed = NewtonGetMemoryUsed() / (1024) ;

		sprintf (statusText, "render fps: (%7.2f)  physics time: (%4.2fms)  bodyCount: (%d)   physicsThreads: (%d)  platform: (%s)  autosleepMode: (%s)    PhysMemory (%d kbytes)", 
							  m_fps, m_scene->GetPhysicsTime() * 1000.0f, NewtonWorldGetBodyCount(world), 
							  NewtonGetThreadsCount(world), platform, m_autoSleepState ? "on" : "off", memoryUsed);

		m_statusbar->getStatusLine()->setNormalText(statusText);

	}
}

void NewtonDemos::RestoreSettings ()
{
	int cpuMode = ((m_cpuInstructionsMode + FXDataTarget::ID_OPTION) & 0xffff) - ID_PLATFORMS;
	NewtonSetCurrentDevice (m_scene->GetNewton(), cpuMode); 

	int threadIndex = ((m_microthreadCount + FXDataTarget::ID_OPTION) & 0xffff) - ID_SELECT_MICROTHREADS;
	NewtonSetThreadsCount(m_scene->GetNewton(), m_mainMenu->m_threadsTracks[threadIndex]); 

	int broadPhase = ((m_broadPhaseMode + FXDataTarget::ID_OPTION) & 0xffff) - ID_DYNAMICS_BROADPHASE;
	NewtonSelectBroadphaseAlgorithm (m_scene->GetNewton(), broadPhase);
}



bool NewtonDemos::GetMousePosition (int& posX, int& posY) const
{
	posX = m_mousePosX;
	posY = m_mousePosY;
	return true;
}

bool NewtonDemos::GetJoytickPosition (dFloat& posX, dFloat& posY, int& buttonsMask) const
{
	buttonsMask = m_joytickButtonMask;
	posX = dFloat (m_joytickX - 32767) / 32768.0f;
	posY = -dFloat (m_joytickY - 32767) / 32768.0f;
	return m_hasJoysticController;
}


bool NewtonDemos::GetMouseKeyState (int button) const
{
	if ((button >= 0) && (button <= 2)) {
		return m_key[m_keyMap[button]] ? true : false;
	}

	return false;
}


bool NewtonDemos::GetKeyState( int key ) const
{
	return m_key[m_keyMap[key & 0xff]] ? true : false;
}


long NewtonDemos::onJoysticMove(FXObject* sender, FXSelector id, void* eventPtr)
{
	FXEvent* const event=(FXEvent*)eventPtr;

	m_hasJoysticController = true;
	m_joytickX = event->win_x;
	m_joytickY = event->win_y;
	m_joytickButtonMask = event->code;
	return 1;
}

long NewtonDemos::onKeyPress(FXObject* sender, FXSelector id, void* eventPtr)
{
	FXEvent* const event=(FXEvent*)eventPtr;
	
	if (event->code == KEY_Escape) {
		getApp()->addChore(this, ID_CLOSE);
	}

	int code = event->code & 0xff; 
	m_key[m_keyMap[code]] = true;
	return 1;
}


long NewtonDemos::onKeyRelease(FXObject* sender, FXSelector id, void* eventPtr)
{
	FXEvent* const event=(FXEvent*)eventPtr;
	int code = event->code & 0xff;
	m_key[m_keyMap[code]] = false;
	return 1;
}



long NewtonDemos::onMouseMove(FXObject* sender, FXSelector id, void* eventPtr)
{
	FXEvent* const event=(FXEvent*)eventPtr;
	m_mousePosX = event->win_x;
	m_mousePosY = event->win_y;
	return 1;
}

void NewtonDemos::LoadDemo (int index)
{
	BEGIN_MENU_OPTION();

	index = FXSELID(index);
	m_scene->Cleanup();
	m_menubar->LoadDemo(m_scene, index - ID_RUN_DEMO);

	RestoreSettings ();
	m_scene->ResetTimer();


	END_MENU_OPTION();
}

long NewtonDemos::onNew(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();
	m_scene->Cleanup();
	RestoreSettings ();
	m_scene->ResetTimer();
	END_MENU_OPTION();
	return 1;
}

long NewtonDemos::onLoad(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();

	const FXchar patterns[]="Newton Dynamics Files (*.ngd)";
	FXFileDialog open(this,"Load Newton Dynamics scene");
	open.setPatternList(patterns);
	open.setDirectory ("../../../media");
	if(open.execute()){

		m_scene->Cleanup();

		// load the scene from a ngd file format
		m_scene->makeCurrent();
		m_scene->LoadScene (open.getFilename().text());
		m_scene->makeNonCurrent();

		// add a sky box to the scene, make the first object
		m_scene->Addtop (new SkyBox());

		// place camera into position
		dMatrix camMatrix (GetIdentityMatrix());
//		camMatrix.m_posit = dVector (-40.0f, 10.0f, 0.0f, 0.0f);
		camMatrix = dYawMatrix(-0.0f * 3.1416f / 180.0f);
		camMatrix.m_posit = dVector (-5.0f, 1.0f, -0.0f, 0.0f);
		m_scene->SetCameraMatrix(camMatrix, camMatrix.m_posit);

		RestoreSettings ();
	}


	m_scene->ResetTimer();
	END_MENU_OPTION();
	return 1;
}

long NewtonDemos::onSave(FXObject* sender, FXSelector id, void* eventPtr)
{
	return 1;
}


long NewtonDemos::onSerializeWorld(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();

	const FXchar patterns[]="Newton Dynamics Files (*.bin)";
	FXFileDialog open(this, "Export a Newton Dynamics Serialized Physics Scene");
	open.setPatternList(patterns);
	open.setDirectory ("../../../media");
	if(open.execute()){
		m_scene->SerializedPhysicScene (open.getFilename().text());
	}

	m_scene->ResetTimer();
	END_MENU_OPTION();
	return 1;
}

long NewtonDemos::onDeserializeWorld(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();

	const FXchar patterns[]="Newton Dynamics Files (*.bin)";
	FXFileDialog open(this, "Import a Newton Dynamics Serialized Physics Scene");
	open.setPatternList(patterns);
	open.setDirectory ("../../../media");
	if(open.execute()){
		m_scene->makeCurrent();
		m_scene->DeserializedPhysicScene (open.getFilename().text());
		m_scene->makeNonCurrent();
		RestoreSettings ();
	}

	m_scene->ResetTimer();
	END_MENU_OPTION();
	return 1;
}



long NewtonDemos::onRunDemo(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();

	LoadDemo (id);

	RestoreSettings ();
	END_MENU_OPTION();

	return 1;
}

long NewtonDemos::onHideVisualMeshes(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();
	FXuval val = FXuval (eventPtr);

	m_hideVisualMeshes = val ? true : false;

	END_MENU_OPTION();
	return 1;
}


long NewtonDemos::onShowConcurrentProfiler(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();
	FXuval val = FXuval (eventPtr);

	m_concurrentProfilerState = val ? true : false;

	END_MENU_OPTION();
	return 1;
}


long NewtonDemos::onShowThreadProfiler(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();
	FXuval val = FXuval (eventPtr);

	m_threadProfilerState = val ? true : false;

	END_MENU_OPTION();
	return 1;
}


long NewtonDemos::onShowProfiler(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();
	FXuval val = FXuval (eventPtr);

	int track = (id & 0xffff)- ID_SHOW_PHYSICS_PROFILER;
	int state = val ? 1 : 0;
	m_scene->m_showProfiler[track] = state;

	END_MENU_OPTION();
	return 1;
}

long NewtonDemos::onSelectAllPerformanceChart(FXObject* sender, FXSelector id, void* eventPtr)
{
	int count = sizeof (m_mainMenu->m_profilerTracksMenu) / sizeof (m_mainMenu->m_profilerTracksMenu[0]);
	for (int i = 0; i < count; i ++) {
		m_scene->m_showProfiler[i] = 1;
		m_mainMenu->m_profilerTracksMenu[i]->setCheck(true);
	}
	return 1;
}

long NewtonDemos::onUnselectAllPerformanceChart(FXObject* sender, FXSelector id, void* eventPtr)
{
	int count = sizeof (m_mainMenu->m_profilerTracksMenu) / sizeof (m_mainMenu->m_profilerTracksMenu[0]);
	for (int i = 0; i < count; i ++) {
		m_scene->m_showProfiler[i] = 0;
		m_mainMenu->m_profilerTracksMenu[i]->setCheck(false);
	}
	return 1;
}

long NewtonDemos::onShowAABB(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();

	FXuval val = FXuval (eventPtr);
	m_showAABB = val ? true : false;
	END_MENU_OPTION();
	return 1;
}

long NewtonDemos::onAutoSleepMode(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();

	FXuval val = FXuval (eventPtr);
	m_autoSleepState = val ? true : false;
	END_MENU_OPTION();
	return 1;
}


long NewtonDemos::onShowContactPoints(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();

	FXuval val = FXuval (eventPtr);
	m_showContactPoints = val ? true : false;
	END_MENU_OPTION();
	return 1;
}

long NewtonDemos::onShowNormalForces(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();

	FXuval val = FXuval (eventPtr);
	m_showNormalForces = val ? true : false;
	END_MENU_OPTION();
	return 1;
}


long NewtonDemos::onShowCollisionLines(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();
	FXuval val = FXuval (eventPtr);

	m_debugDisplayState = val ? true : false;
	SetDebugDisplayMode (m_debugDisplayState);

	END_MENU_OPTION();
	return 1;
}



long NewtonDemos::onRunPhysicsConcurrent(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();

	FXuval val = FXuval (eventPtr);
	m_physicsUpdateMode = val ? true : false;

	END_MENU_OPTION();
	return 1;
}


long NewtonDemos::onSelectNumberOfMicroThreads(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();
	int selection = ((m_microthreadCount + FXDataTarget::ID_OPTION) & 0xffff) - ID_SELECT_MICROTHREADS;

	NewtonSetThreadsCount(m_scene->GetNewton(), m_mainMenu->m_threadsTracks[selection]);

	END_MENU_OPTION();
	return 1;
}


long NewtonDemos::onUseParallelSolver(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();

	FXuval val = FXuval (eventPtr);
	m_useParallelSolver =  val ? true : false;

	NewtonSetMultiThreadSolverOnSingleIsland (m_scene->GetNewton(), m_useParallelSolver ? 1 : 0);

	END_MENU_OPTION();
	return 1;
}


long NewtonDemos::onBroadPhaseType(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();
	int selection = ((m_broadPhaseMode + FXDataTarget::ID_OPTION) & 0xffff) - ID_DYNAMICS_BROADPHASE;

	_ASSERTE (selection >= 0);
	_ASSERTE (selection <= 3);
	NewtonSelectBroadphaseAlgorithm (m_scene->GetNewton(), selection);

	END_MENU_OPTION();
	return 1;
}

long NewtonDemos::onSimdInstructions(FXObject* sender, FXSelector id, void* eventPtr)
{
	BEGIN_MENU_OPTION();
	int selection = ((m_cpuInstructionsMode + FXDataTarget::ID_OPTION) & 0xffff) - ID_PLATFORMS;
	NewtonSetCurrentDevice (m_scene->GetNewton(), selection);
	END_MENU_OPTION();
	return 1;
}



long NewtonDemos::onChore(FXObject* sender, FXSelector id, void* eventPtr)
{
	onPaint(sender, id, eventPtr);
	getApp()->addChore(this, ID_CHORE);
	return 1;
}

long NewtonDemos::onPaint(FXObject* sender, FXSelector id, void* eventPtr)
{
	if (m_doVisualUpdates) {
		// render scene
		dFloat timestep = dGetElapsedSeconds();	
		m_scene->UpdateScene(timestep);

		CalculateFPS(timestep);
	}
	return 1;
}

