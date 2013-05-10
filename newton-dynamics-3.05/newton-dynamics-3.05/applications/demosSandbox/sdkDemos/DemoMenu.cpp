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

#include "toolbox_stdafx.h"
#include "DemoMenu.h"
#include "NewtonDemos.h"
#include "DemoEntityManager.h"

void Friction (DemoEntityManager* const scene);
void Restitution (DemoEntityManager* const scene);
void PrecessingTops (DemoEntityManager* const scene);
void ClosestDistance (DemoEntityManager* const scene);
void ConvexCast (DemoEntityManager* const scene);
void PrimitiveCollision (DemoEntityManager* const scene);
void KinematicBodies (DemoEntityManager* const scene);
void SoftBodies (DemoEntityManager* const scene);
void BasicBoxStacks (DemoEntityManager* const scene);
void SimpleMeshLevelCollision (DemoEntityManager* const scene);
void OptimizedMeshLevelCollision (DemoEntityManager* const scene);
void UniformScaledCollision (DemoEntityManager* const scene);
void NonUniformScaledCollision (DemoEntityManager* const scene);
void ScaledMeshCollision (DemoEntityManager* const scene);
void ContinueCollision (DemoEntityManager* const scene);
void PuckSlide (DemoEntityManager* const scene);
void SceneCollision (DemoEntityManager* const scene);
void CompoundCollision(DemoEntityManager* const scene);
void PostCompoundCreateBuildTest(DemoEntityManager* const scene);
void SimpleConvexApproximation(DemoEntityManager* const scene);
void SimpleBooleanOperations(DemoEntityManager* const scene);
void SimpleConvexShatter (DemoEntityManager* const scene);
void UsingNewtonMeshTool (DemoEntityManager* const scene);
void MultiRayCast (DemoEntityManager* const scene);
void BasicCar (DemoEntityManager* const scene);
void BasicPlayerController (DemoEntityManager* const scene);
void AdvancedPlayerController (DemoEntityManager* const scene);
void SuperCar (DemoEntityManager* const scene);
void HeightFieldCollision (DemoEntityManager* const scene);
void UserPlaneCollision (DemoEntityManager* const scene);
void UserHeightFieldCollision (DemoEntityManager* const scene);



SDKDemos DemoMenu::m_demosSelection[] = 
{
	{"Using the newton mesh tool", "demonstrate how to use the newton mesh toll for mesh manipulation", UsingNewtonMeshTool},
	{"Coefficients of friction", "demonstrate the effect of various coefficient of friction", Friction},
	{"Coefficients of restitution", "demonstrate the effect of various coefficient of restitution", Restitution},
	{"Precessing tops", "show natural precession", PrecessingTops},
	{"Closest distance", "demonstrate closest distance to a convex shape", ClosestDistance},
	{"Primitive Collision", "demonstrate separate collision of primitives", PrimitiveCollision},
	{"Kinematic bodies", "demonstrate separate collision of primitives", KinematicBodies},
	{"Primitive convex cast", "demonstrate separate primitive convex cast", ConvexCast},
	{"Simple box Stacks", "show simple stack of Boxes", BasicBoxStacks},
	{"Unoptimized mesh collision", "show simple level mesh", SimpleMeshLevelCollision},
	{"Optimized mesh collision", "show optimized level mesh", OptimizedMeshLevelCollision},
	{"Height field collision mesh", "show high file collision mesh", HeightFieldCollision},
	{"User infinite Plane collision mesh", "show high file collision mesh", UserPlaneCollision},
	{"User Height field collision mesh", "show high file collision mesh", UserHeightFieldCollision},
	{"Compound collision shape", "demonstrate compound collision", CompoundCollision},
	{"PostCompoundCreateBuildTest", "PostCompoundCreateBuildTest", PostCompoundCreateBuildTest},
	{"Uniform scaled collision shape", "demonstrate scaling shape", UniformScaledCollision},
	{"Non uniform scaled collision shape", "demonstrate scaling shape", NonUniformScaledCollision},
	{"Scaled mesh collision", "demonstrate scaling mesh scaling collision", ScaledMeshCollision},
	{"Simple convex decomposition", "demonstrate convex decomposition and compound collision", SimpleConvexApproximation},
	{"Multi geometry collision", "show static mesh with the ability of moving internal parts", SceneCollision},
	{"Simple boolean operations", "demonstrate simple boolean operations ", SimpleBooleanOperations},
	{"Simple convex Shatter", "demonstrate fracture destruction using voronoi partition", SimpleConvexShatter},
	{"Parallel ray cast", "using the threading Job scheduler", MultiRayCast},
	{"Continue collision", "show continue collision", ContinueCollision},
	{"Puck slide", "show continue collision", PuckSlide},
	{"Basic Car", "implement a basic car", BasicCar},
	{"Basic player controller", "demonstrate simple player controller", BasicPlayerController},
	{"Advanced player controller", "demonstrate player interacting with other objects", AdvancedPlayerController},
//	{"High performance super car", "implement a high performance ray cast car", SuperCar},
//	{"Simple soft Body", "show simple stack of Boxes", SoftBodies},
	

//	{"basic convex hull stacking", "demonstrate convex hull stacking", BasicConvexStacks},
//	{"basic unstable stacking", "demonstrate stability stacking unstable objects", UnstableStacks},
//	{"Jenga stacking", "demonstrate Jenga game", Jenga},
//	{"Large Jenga stacking", "demonstrate Jenga game", JengaTall},
//	{"small pyramid stacking", "demonstrate small pyramid stacking", CreatePyramid},
//	{"wall stacking", "demonstrate wall stacking", CreateWalls},
//	{"small tower stacking", "demonstrate tower stacking", CreateTower},
//	{"large tower stacking", "demonstrate tower stacking", CreateTowerTall},
//	{"user defined polygon static collision", "demonstrate user defined polygon static collision", UserHeighMapColliion},
//	{"attractive magnets force field", "demonstrate attractive force field", Magnets},
//	{"repulsive magnets force field", "demonstrate repulsive magnet force field", Repulsive},
//	{"Archimedes buoyancy force field", "demonstrate user define Archimedes as force field", ArchimedesBuoyancy},
//	{"legacy joints", "demonstrate the build in joints", LegacyJoints},
//	{"custom joints", "demonstrate custom joints", BasicCustomJoints},
//	{"Simple robots", "demonstrate custom joints robot", BasicRobots},
//	{"motorized robots", "demonstrate motorized custom joints robot", TracktionJoints},
//	{"discrete rag doll", "demonstrate simple rag doll", DescreteRagDoll},
//	{"skinned rag doll", "demonstrate simple rag doll", SkinRagDoll},
};


DemoMenu::DemoMenu(FXComposite* const parent, NewtonDemos* const mainFrame)
	:FXMenuBar(parent, new FXToolBarShell(mainFrame, FRAME_RAISED), LAYOUT_DOCK_SAME|LAYOUT_SIDE_TOP|LAYOUT_FILL_X|FRAME_RAISED)
{
	new FXToolBarGrip(this, this, FXMenuBar::ID_TOOLBARGRIP,TOOLBARGRIP_DOUBLE);

	m_threadsTracks[0] = 1;
	m_threadsTracks[1] = 2;
	m_threadsTracks[2] = 3;
	m_threadsTracks[3] = 4;
	m_threadsTracks[4] = 8;
	m_threadsTracks[5] = 12;
	m_threadsTracks[6] = 16;


	// adding the file menu
	{
		m_fileMenu = new FXMenuPane(this);
		new FXMenuTitle(this, "File", NULL, m_fileMenu);

		new FXMenuCommand(m_fileMenu, "New", NULL, mainFrame, NewtonDemos::ID_NEW);
		new FXMenuSeparator(m_fileMenu);

		//new FXMenuCommand(m_fileMenu, "Load", NULL, mainFrame, NewtonDemos::ID_LOAD);
		//new FXMenuCommand(m_fileMenu, "Save", NULL, mainFrame, NewtonDemos::ID_SAVE);
		new FXMenuSeparator(m_fileMenu);

		new FXMenuCommand(m_fileMenu, "Import serialize scene", NULL, mainFrame, NewtonDemos::ID_DESERIALIZE);
		new FXMenuCommand(m_fileMenu, "Export serialize scene", NULL, mainFrame, NewtonDemos::ID_SERIALIZE);
		new FXMenuSeparator(m_fileMenu);

		new FXMenuCommand(m_fileMenu, "Quit", NULL, getApp(), FXApp::ID_QUIT);
	}

	// engine examples demos  
	{
		m_demosMenu = new FXMenuPane(this);
		new FXMenuTitle(this, "Demos", NULL, m_demosMenu);

		// add all demos
		FXMenuCommand* demoList[256];
		int demosCount = int (sizeof (m_demosSelection) / sizeof m_demosSelection[0]);
		for (int i = 0; i < demosCount; i ++) {
			demoList[i] = new FXMenuCommand(m_demosMenu, m_demosSelection[i].m_name, NULL, mainFrame, NewtonDemos::ID_RUN_DEMO + i);
		}

		// disable demos that still do not work 
		for (int i = 0; i < 3; i ++) {
//			demoList[demosCount - i - 1]->disable();
		}
	}


	// option menu
	{
		m_optionsMenu = new FXMenuPane(this);
		new FXMenuTitle(this, "Options", NULL, m_optionsMenu);

		new FXMenuCheck(m_optionsMenu, "Autosleep off", mainFrame, NewtonDemos::ID_AUTOSLEEP_MODE);
		new FXMenuCheck(m_optionsMenu, "Hide visual meshes", mainFrame, NewtonDemos::ID_HIDE_VISUAL_MESHES);
		new FXMenuCheck(m_optionsMenu, "Show collision Mesh", mainFrame, NewtonDemos::ID_SHOW_COLLISION_MESH);
		new FXMenuCheck(m_optionsMenu, "Show contact points", mainFrame, NewtonDemos::ID_SHOW_CONTACT_POINTS);
		new FXMenuCheck(m_optionsMenu, "Show normal forces", mainFrame, NewtonDemos::ID_SHOW_NORMAL_FORCES);
		new FXMenuCheck(m_optionsMenu, "Show aabb", mainFrame, NewtonDemos::ID_SHOW_AABB);
		new FXMenuCheck(m_optionsMenu, "Parallel solver on", mainFrame, NewtonDemos::ID_USE_PARALLEL_SOLVER);

		new FXMenuSeparator(m_optionsMenu);
//		m_cpuModes[0] = new FXMenuRadio(m_optionsMenu, "Use x87 instructions", &mainFrame->m_cpuInstructionSelection, NewtonDemos::ID_USE_X87_INSTRUCTIONS);
//		m_cpuModes[1] = new FXMenuRadio(m_optionsMenu, "Use sse instructions", &mainFrame->m_cpuInstructionSelection, NewtonDemos::ID_USE_SIMD_INSTRUCTIONS);
//		m_cpuModes[2] = new FXMenuRadio(m_optionsMenu, "Use avx instructions", &mainFrame->m_cpuInstructionSelection, NewtonDemos::ID_USE_AVX_INSTRUCTIONS);
		if (mainFrame->m_scene) {
			int platformsCount = NewtonEnumrateDevices (mainFrame->m_scene->GetNewton());
			for (int i = 0; i < platformsCount; i ++) {
				char platform[256];
				NewtonGetDeviceString (mainFrame->m_scene->GetNewton(), i, platform, sizeof (platform));
				m_cpuModes[i] = new FXMenuRadio(m_optionsMenu, platform, &mainFrame->m_cpuInstructionSelection, NewtonDemos::ID_PLATFORMS + i);
			}
		}

		new FXMenuSeparator(m_optionsMenu);
		new FXMenuRadio(m_optionsMenu, "dynamics broad phase", &mainFrame->m_broadPhaseSelection, NewtonDemos::ID_DYNAMICS_BROADPHASE);
		new FXMenuRadio(m_optionsMenu, "static broad phase", &mainFrame->m_broadPhaseSelection, NewtonDemos::ID_STATIC_BROADPHASE);
		new FXMenuRadio(m_optionsMenu, "hybrid broad phase", &mainFrame->m_broadPhaseSelection, NewtonDemos::ID_HYBRID_BROADPHASE);


		new FXMenuSeparator(m_optionsMenu);
		FXMenuCheck* const concurrentProfiler = new FXMenuCheck(m_optionsMenu, "Show concurrent profiler", mainFrame, NewtonDemos::ID_SHOW_CONCURRENCE_PROFILER);
		if (mainFrame->m_concurrentProfilerState) {
			concurrentProfiler->setCheck(true);
		}
		FXMenuCheck* const threadProfiler = new FXMenuCheck(m_optionsMenu, "Show micro thread profiler", mainFrame, NewtonDemos::ID_SHOW_PROFILER);
		if (mainFrame->m_threadProfilerState) {
			threadProfiler->setCheck(true);
		}

		new FXMenuSeparator(m_optionsMenu);
		new FXMenuCommand(m_optionsMenu, "select all profiler", NULL, mainFrame, NewtonDemos::ID_SELECT_ALL_PROFILERS);
		new FXMenuCommand(m_optionsMenu, "unselect all profiler", NULL, mainFrame, NewtonDemos::ID_UNSELECT_ALL_PROFILERS);

		m_profilerSubMenu = new FXMenuPane(this);
		m_profilerTracksMenu[0] = new FXMenuCheck(m_profilerSubMenu, "show global physics update performance chart", mainFrame, NewtonDemos::ID_SHOW_PHYSICS_PROFILER + 0);
		m_profilerTracksMenu[1] = new FXMenuCheck(m_profilerSubMenu, "global collision update performance chart", mainFrame, NewtonDemos::ID_SHOW_PHYSICS_PROFILER + 1);
		m_profilerTracksMenu[2] = new FXMenuCheck(m_profilerSubMenu, "broad phase collision performance chart", mainFrame, NewtonDemos::ID_SHOW_PHYSICS_PROFILER + 2);
		m_profilerTracksMenu[3] = new FXMenuCheck(m_profilerSubMenu, "narrow phase collision performance chart", mainFrame, NewtonDemos::ID_SHOW_PHYSICS_PROFILER + 3);
		m_profilerTracksMenu[4] = new FXMenuCheck(m_profilerSubMenu, "global dynamics update performance chart", mainFrame, NewtonDemos::ID_SHOW_PHYSICS_PROFILER + 4);
		m_profilerTracksMenu[5] = new FXMenuCheck(m_profilerSubMenu, "dynamics setup performance chart", mainFrame, NewtonDemos::ID_SHOW_PHYSICS_PROFILER + 5);
		m_profilerTracksMenu[6] = new FXMenuCheck(m_profilerSubMenu, "dynamics solver performance chart", mainFrame, NewtonDemos::ID_SHOW_PHYSICS_PROFILER + 6);
		m_profilerTracksMenu[7] = new FXMenuCheck(m_profilerSubMenu, "force and torque callback performance chart", mainFrame, NewtonDemos::ID_SHOW_PHYSICS_PROFILER + 7);
		m_profilerTracksMenu[8] = new FXMenuCheck(m_profilerSubMenu, "pre simulation listener", mainFrame, NewtonDemos::ID_SHOW_PHYSICS_PROFILER + 8);
		m_profilerTracksMenu[9] = new FXMenuCheck(m_profilerSubMenu, "post simulation listener", mainFrame, NewtonDemos::ID_SHOW_PHYSICS_PROFILER + 9);
		if (mainFrame->m_physicProfilerState) {
			m_profilerTracksMenu[0]->setCheck(true);
		}
		new FXMenuCascade(m_optionsMenu, "select sub profiler", NULL, m_profilerSubMenu);


		new FXMenuSeparator(m_optionsMenu);
		new FXMenuCheck(m_optionsMenu, "Concurrent physics update", mainFrame, NewtonDemos::ID_CONCURRENT_PHYSICS_UPDATE);
		m_microThreadedsSubMenu = new FXMenuPane(this);
		FXMenuRadio* threadMenus[128];
		for (int i = 0 ; i < int (sizeof (m_threadsTracks)/ sizeof (m_threadsTracks[0])); i ++) {
			char label[1024];
			sprintf (label, "%d micro threads", m_threadsTracks[i]);
			threadMenus[i] = new FXMenuRadio(m_microThreadedsSubMenu, label, &mainFrame->m_microthreadCountSelection, NewtonDemos::ID_SELECT_MICROTHREADS + i);
		}
		threadMenus[0]->setCheck(true);
		new FXMenuCascade(m_optionsMenu, "select microThread count", NULL, m_microThreadedsSubMenu);
		
	}

	// add help menu
	{
		m_helpMenu = new FXMenuPane(this);
		new FXMenuTitle(this, "Help", NULL, m_helpMenu);
		new FXMenuCommand(m_helpMenu, "About", NULL, mainFrame, NewtonDemos::ID_ABOUT);
	}
}

DemoMenu::~DemoMenu(void)
{
	delete m_microThreadedsSubMenu;
	delete m_profilerSubMenu;
	
	delete m_demosMenu;
	delete m_optionsMenu;
	delete m_helpMenu;
	delete m_fileMenu;
}

void DemoMenu::LoadDemo (DemoEntityManager* const scene, int index)
{
	scene->makeCurrent();
	m_demosSelection[index].m_launchDemoCallback (scene);
	scene->swapBuffers();  // added this line
	scene->makeNonCurrent();
}

