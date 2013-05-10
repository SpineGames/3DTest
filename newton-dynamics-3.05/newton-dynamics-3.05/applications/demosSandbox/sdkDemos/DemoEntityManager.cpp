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
#include "NewtonDemos.h"
#include "DemoEntityManager.h"
#include "SkyBox.h"
#include "DemoMesh.h"
#include "DemoEntity.h"
#include "DemoCamera.h"
#include "PhysicsUtils.h"
#include "DebugDisplay.h"
#include "dRuntimeProfiler.h"
#include "OpenGlUtil.h"

#include "DemoAIListener.h"
#include "DemoSoundListener.h"
#include "DemoEntityListener.h"
#include "DemoCameraListener.h"
#include "DemoVisualDebugerListener.h"
#include "CustomPlayerControllerManager.h"
#include "CustomVehicleControllerManager.h"

#define MAX_PHYSICS_LOOPS			1
#define MAX_PHYSICS_FPS				120.0f
#define PROJECTILE_INITIAL_SPEED	20.0f


FXDEFMAP(DemoEntityManager) MessageMap[]=
{
	FXMAPFUNC(SEL_LEFTBUTTONPRESS,		0,	DemoEntityManager::onLeftMouseKeyDown),
	FXMAPFUNC(SEL_LEFTBUTTONRELEASE,	0,	DemoEntityManager::onLeftMouseKeyUp),
	FXMAPFUNC(SEL_RIGHTBUTTONPRESS,		0,	DemoEntityManager::onRightMouseKeyDown),
	FXMAPFUNC(SEL_RIGHTBUTTONRELEASE,	0,	DemoEntityManager::onRightMouseKeyUp),

	FXMAPFUNC(SEL_LEAVE,                0,	DemoEntityManager::onMouseLeave),
};
FXIMPLEMENT(DemoEntityManager,FXGLCanvas,MessageMap,ARRAYNUMBER(MessageMap))




DemoEntityManager::ButtonKey::ButtonKey (bool state)
	:m_state(state)
	,m_memory0(false)
	,m_memory1(false)
{
}

bool DemoEntityManager::ButtonKey::UpdateTriggerButton (const NewtonDemos* const mainWin, int keyCode)
{
	m_memory0 = m_memory1;
	m_memory1 = mainWin->GetKeyState (keyCode);
	return !m_memory0 & m_memory1;
}

bool DemoEntityManager::ButtonKey::UpdateTriggerJoystick (const NewtonDemos* const mainWin, int buttonMask)
{
	m_memory0 = m_memory1;
	m_memory1 = buttonMask ? true : false;
	return !m_memory0 & m_memory1;
}

void DemoEntityManager::ButtonKey::UpdatePushButton (const NewtonDemos* const mainWin, int keyCode)
{
	if (UpdateTriggerButton (mainWin, keyCode)) {
		m_state = ! m_state;
	}
}


class GLVisual: public FXGLVisual
{
	public:
	GLVisual (FXApp* const app)
		:FXGLVisual (app, VISUAL_DOUBLEBUFFER)
	{
	}
};

DemoEntityManager::DemoEntityManager()
	:FXGLCanvas ()
{

}

DemoEntityManager::DemoEntityManager(FXComposite* const parent, NewtonDemos* const mainFrame)
	:FXGLCanvas (parent, new GLVisual (mainFrame->getApp()), mainFrame, NewtonDemos::ID_CANVAS, LAYOUT_FILL_X | LAYOUT_FILL_Y | LAYOUT_TOP | LAYOUT_LEFT)
	,dList <DemoEntity*>() 
	,m_mainWindow(mainFrame)
	,m_world(NULL)
	,m_aiWorld(NULL)
	,m_sky(NULL)
	,m_microsecunds(0)
	,m_physicsTime(0.0f)
	,m_currentListenerTimestep(0.0f)
	,m_physicsUpdate(true) 
	,m_reEntrantUpdate (false)
	,m_renderHoodContext(NULL)
	,m_renderHood(NULL)
	,m_font(0)
	,m_soundManager(NULL)
	,m_cameraManager(NULL)
	,m_profiler(620 * 0 / 8 + 45, 40)
	,m_mainThreadGraphicsTime(0.0f)
	,m_mainThreadPhysicsTime(0.0f)
	,m_physThreadTime(0.0f)
{

	// initialized the physics world for the new scene
	Cleanup ();

	ResetTimer();

	// Set performance counters off
	memset (m_showProfiler, 0, sizeof (m_showProfiler));
	m_profiler.Init(this);
}


DemoEntityManager::~DemoEntityManager(void)
{
	// is we are run asynchronous we need make sure no update in on flight.
	if (m_world) {
		NewtonWaitForUpdateToFinish (m_world);
	}


	glDeleteLists(m_font, 96);	

	Cleanup ();

	// destroy the empty world
	if (m_world) {
		NewtonDestroy (m_world);
		m_world = NULL;
	}
	_ASSERTE (NewtonGetMemoryUsed () == 0);

	// delete GUI elements
	delete getVisual();
}


void DemoEntityManager::Set2DDisplayRenderFunction (RenderHoodCallback callback, void* const context)
{
	m_renderHood = callback;
	m_renderHoodContext = context;
}

void DemoEntityManager::create()
{
	FXGLCanvas::create();

	makeCurrent();
	GLenum err = glewInit();
	if (err == GLEW_OK) {
#ifdef WIN32
		wglSwapIntervalEXT(0);
#else
		glXSwapIntervalSGI(0);  //NOTE check for GLX_SGI_swap_control extension : http://www.opengl.org/wiki/Swap_Interval#In_Linux_.2F_GLX
#endif
	}

	// create a fond for print in 3d window
/*
	HFONT font = CreateFont(
		20,                        // nHeight
		0,                         // nWidth
		0,                         // nEscapement
		0,                         // nOrientation
		FW_BOLD,                   // nWeight
		FALSE,                     // bItalic
		FALSE,                     // bUnderline
		0,                         // cStrikeOut
		ANSI_CHARSET,              // nCharSet
		OUT_DEFAULT_PRECIS,        // nOutPrecision
		CLIP_DEFAULT_PRECIS,       // nClipPrecision
		DEFAULT_QUALITY,           // nQuality
		DEFAULT_PITCH | FF_SWISS,  // nPitchAndFamily
		_T("Arial"));              // lpszFacename


	// only 96 usable ascii for performance in some low end system	
	GLuint displayLists = glGenLists(96);	

	// remember the old font
	HFONT oldfont = (HFONT)SelectObject  (m_hDC, font);	
	wglUseFontBitmaps(m_hDC, 32, 96, displayLists);				
	SelectObject(m_hDC, oldfont);						
	DeleteObject(font);									
	return displayLists;
*/
	m_font = glGenLists(96);	

	FXFont* const font = new FXFont(getApp(), "areal");
	font->create();
	glUseFXFont(font, 32, 96, m_font);
	delete font;

	makeNonCurrent();
}

void DemoEntityManager::CreateSkyBox()
{
	if (!m_sky) {
		m_sky = new SkyBox();
		Append(m_sky);
	}
}

void DemoEntityManager::Print (const dVector& color, dFloat x, dFloat y, const char *fmt, ... ) const
{
	glColor3f(color.m_x, color.m_y, color.m_z);

	glPushMatrix();

	glMatrixMode(GL_PROJECTION);
	glPushMatrix();
	glLoadIdentity();
	glOrtho(0.0, getWidth(), getHeight(), 0.0, 0.0, 1.0);

	glMatrixMode(GL_MODELVIEW);
	glPushMatrix();
	glLoadIdentity();
	glRasterPos2f(x, y + 16);

	va_list argptr;
	char string[1024];

	va_start (argptr, fmt);
	vsprintf (string, fmt, argptr);
	va_end( argptr );


	glPushAttrib(GL_LIST_BIT);
	glListBase(m_font - 32);	
	int lenght = (int) strlen (string);
	glCallLists (lenght, GL_UNSIGNED_BYTE, string);	
	glPopAttrib();				


	glMatrixMode(GL_MODELVIEW);
	glPopMatrix();

	glMatrixMode(GL_PROJECTION);
	glPopMatrix();

	glMatrixMode(GL_MODELVIEW);
	glPopMatrix();
	glLoadIdentity();
}


long DemoEntityManager::onLeftMouseKeyDown(FXObject* sender, FXSelector id, void* eventPtr)
{
	m_mainWindow->m_key[m_mainWindow->m_keyMap[0]] = true;
	return 1;
}

long DemoEntityManager::onLeftMouseKeyUp(FXObject* sender, FXSelector id, void* eventPtr)
{
	m_mainWindow->m_key[m_mainWindow->m_keyMap[0]] = false;
	return 1;
}

long DemoEntityManager::onRightMouseKeyDown(FXObject* sender, FXSelector id, void* eventPtr)
{
	m_mainWindow->m_key[m_mainWindow->m_keyMap[2]] = true;
	return 1;
}

long DemoEntityManager::onRightMouseKeyUp(FXObject* sender, FXSelector id, void* eventPtr)
{
	m_mainWindow->m_key[m_mainWindow->m_keyMap[2]] = false;
	return 1;
}

long DemoEntityManager::onMouseLeave(FXObject* sender, FXSelector id, void* eventPtr)
{
	onLeftMouseKeyUp (sender, id, eventPtr);
	onRightMouseKeyUp(sender, id, eventPtr);
	return 1;
}



float DemoEntityManager::GetPhysicsTime()
{
	return m_physicsTime;
}


void DemoEntityManager::ResetTimer()
{
	dResetTimer();
	m_microsecunds = dGetTimeInMicrosenconds ();
}


DemoCamera* DemoEntityManager::GetCamera() const
{
	return m_cameraManager->GetCamera();
}


void DemoEntityManager::SetCameraMatrix (const dQuaternion& rotation, const dVector& position)
{
	m_cameraManager->SetCameraMatrix(this, rotation, position);
}


void DemoEntityManager::RemoveEntity (dListNode* const entNode)
{
	DemoEntity* const entity = entNode->GetInfo();
	entity->Release();
	Remove(entNode);
}

void DemoEntityManager::RemoveEntity (DemoEntity* const ent)
{
	for (dListNode* node = GetFirst(); node; node = node->GetNext()) {
		if (node->GetInfo() == ent) {
			RemoveEntity (node);
			break;
		}
	}
}


void DemoEntityManager::Cleanup ()
{

	// is we are run asynchronous we need make sure no update in on flight.
	if (m_world) {
		NewtonWaitForUpdateToFinish (m_world);
	}

	// destroy all remaining visual objects
	while (GetFirst()) {
		RemoveEntity (GetFirst());
	}

	m_sky = NULL;

	// destroy the Newton world
	if (m_world) {
		NewtonDestroy (m_world);
		m_world = NULL;
	}

	//	memset (&demo, 0, sizeof (demo));
	// check that there are no memory leak on exit
	_ASSERTE (NewtonGetMemoryUsed () == 0);


	// create the newton world
	m_world = NewtonCreate();

	// link the work with this user data
	NewtonWorldSetUserData(m_world, this);

	// add all physics pre and post listeners
	//	m_preListenerManager.Append(new DemoVisualDebugerListener("visualDebuger", m_world));
	new DemoEntityListener (this);
	m_cameraManager = new DemoCameraListener(this);
	m_soundManager = new DemoSoundListener(this);
//	m_postListenerManager.Append (new DemoAIListener("aiManager"));

	// set the default parameters for the newton world
	// set the simplified solver mode (faster but less accurate)
	NewtonSetSolverModel (m_world, 1);

	// newton 300 does not have world size, this is better controlled by the client application
	//dVector minSize (-500.0f, -500.0f, -500.0f);
	//dVector maxSize ( 500.0f,  500.0f,  500.0f);
	//NewtonSetWorldSize (m_world, &minSize[0], &maxSize[0]); 

	// set the performance track function
	NewtonSetPerformanceClock (m_world, dRuntimeProfiler::GetTimeInMicrosenconds);

	// clean up all caches the engine have saved
	NewtonInvalidateCache (m_world);

	// Set the Newton world user data
	NewtonWorldSetUserData(m_world, this);


	// we start without 2d render
	m_renderHood = NULL;
	m_renderHoodContext = NULL;
}


void DemoEntityManager::LoadVisualScene(dScene* const scene, EntityDictionary& dictionary)
{
	// load all meshes into a Mesh cache for reuse
	dTree<DemoMesh*, dScene::dTreeNode*> meshDictionary;
	for (dScene::dTreeNode* node = scene->GetFirstNode (); node; node = scene->GetNextNode (node)) {
		dNodeInfo* info = scene->GetInfoFromNode(node);
		if (info->GetTypeId() == dMeshNodeInfo::GetRttiType()) {
			DemoMesh* const mesh = new DemoMesh(scene, node);
			meshDictionary.Insert(mesh, node);
		}
	}


	// create an entity for every root node in the mesh
	// a root node or scene entity is a dSceneNodeInfo with a direct link to the root of the dScene node.
	dScene::dTreeNode* const root = scene->GetRootNode();
	for (void* child = scene->GetFirstChild(root); child; child = scene->GetNextChild (root, child)) {
		dScene::dTreeNode* node = scene->GetNodeFromLink(child);
		dNodeInfo* info = scene->GetInfoFromNode(node);
		if (info->GetTypeId() == dSceneNodeInfo::GetRttiType()) {
			// we found a root dSceneNodeInfo, convert it to a Scene entity and load all it children 
			DemoEntity* const entityRoot = new DemoEntity (*this, scene, node, meshDictionary, dictionary);
			Append(entityRoot);
		}
	}

	// release all meshes before exiting
	dTree<DemoMesh*, dScene::dTreeNode*>::Iterator iter (meshDictionary);
	for (iter.Begin(); iter; iter++) {
		DemoMesh* const mesh = iter.GetNode()->GetInfo();
		mesh->Release();
	}
}


void DemoEntityManager::LoadScene (const char* const fileName)
{
	dScene database (GetNewton());

	database.Deserialize(fileName);

	// this will apply all global the scale to the mesh
	database.FreezeScale();
	// this will apply all local scale and transform to the mesh
	database.FreezePivot();

	// Load the Visual Scene
	EntityDictionary entDictionary;
	LoadVisualScene(&database, entDictionary);

	//Load the physics world
	dList<NewtonBody*> bodyList;
	database.SceneToNewtonWorld(m_world, bodyList);

	// bind every rigidBody loaded to the scene entity
	for (dList<NewtonBody*>::dListNode* bodyNode = bodyList.GetFirst(); bodyNode; bodyNode = bodyNode->GetNext()) {
		// find the user data and set to the visual entity in the scene
		NewtonBody* const body = bodyNode->GetInfo();
		dScene::dTreeNode* const sceneNode = (dScene::dTreeNode*)NewtonBodyGetUserData(body);
		DemoEntity* const entity = entDictionary.Find(sceneNode)->GetInfo();
		NewtonBodySetUserData(body, entity);

		// see if this body have some special setups
		dScene::dTreeNode* const node = database.FindChildByType(sceneNode, dRigidbodyNodeInfo::GetRttiType());
		_ASSERTE (node);
		dRigidbodyNodeInfo* const bodyData = (dRigidbodyNodeInfo*) database.GetInfoFromNode(node);
		dVariable* bodyType = bodyData->FindVariable("rigidBodyType");

		// set the default call backs
		if (!bodyType || !strcmp (bodyType->GetString(), "default gravity")) {
			NewtonBodySetTransformCallback(body, DemoEntity::TransformCallback);
			NewtonBodySetForceAndTorqueCallback(body, PhysicsApplyGravityForce);
			NewtonBodySetDestructorCallback (body, PhysicsBodyDestructor);
		}
	}

	// clean up all caches the engine have saved
	NewtonInvalidateCache (m_world);
}


void DemoEntityManager::SerializeFile (void* const serializeHandle, const void* const buffer, int size)
{
	// check that each chunk is a multiple of 4 bytes, this is useful for easy little to big Indian conversion
	_ASSERTE ((size & 0x03) == 0);
	fwrite (buffer, size, 1, (FILE*) serializeHandle);
}

void DemoEntityManager::DeserializeFile (void* const serializeHandle, void* const buffer, int size)
{
	// check that each chunk is a multiple of 4 bytes, this is useful for easy little to big Indian conversion
	_ASSERTE ((size & 0x03) == 0);
	fread (buffer, size, 1, (FILE*) serializeHandle);
}


void DemoEntityManager::BodySerialization (NewtonBody* const body, NewtonSerializeCallback serializeCallback, void* const serializeHandle)
{
	// here the use can save information of this body, ex:
	// a string naming the body,  
	// serialize the visual mesh, or save a link to the visual mesh
	// save labels for looking up the body call backs

	// for the demos I will simple write three stream to identify what body it is, the application can do anything
	const char* const bodyIndentification = "gravityBody\0\0\0\0";
	int size = (strlen (bodyIndentification) + 3) & -4;
	serializeCallback (serializeHandle, &size, sizeof (size));
	serializeCallback (serializeHandle, bodyIndentification, size);
}


void DemoEntityManager::BodyDeserialization (NewtonBody* const body, NewtonDeserializeCallback deserializecallback, void* const serializeHandle)
{
	int size;
	char bodyIndentification[256];
	
	deserializecallback (serializeHandle, &size, sizeof (size));
	deserializecallback (serializeHandle, bodyIndentification, size);


	// get the world and the scene form the world user data
	NewtonWorld* const world = NewtonBodyGetWorld(body);
	DemoEntityManager* const scene = (DemoEntityManager*)NewtonWorldGetUserData(world);

	// here we attach a visual object to the entity, 
	dMatrix matrix;
	NewtonBodyGetMatrix(body, &matrix[0][0]);
	DemoEntity* const entity = new DemoEntity(matrix, NULL);
	scene->Append (entity);

	NewtonBodySetUserData (body, entity);
	NewtonBodySetTransformCallback(body, DemoEntity::TransformCallback);
	NewtonBodySetForceAndTorqueCallback(body, PhysicsApplyGravityForce);


	//for visual mesh we will collision mesh and convert it to a visual mesh using NewtonMesh 
	NewtonCollision* const collision = NewtonBodyGetCollision(body);
	DemoMesh* const mesh = new DemoMesh(bodyIndentification, collision, NULL, NULL, NULL);
/*
	int collisionID = NewtonCollisionGetType(collision) ;
	DemoMesh* mesh = NULL;
	switch (collisionID) 
	{
		case SERIALIZE_ID_HEIGHTFIELD:
		{
			NewtonCollisionInfoRecord info;
			NewtonCollisionGetInfo(collision, &info);

			const NewtonHeightFieldCollisionParam& heighfield = info.m_heightField;
			mesh = new DemoMesh ("terrain", heighfield.m_elevation, heighfield.m_width, heighfield.m_horizonalScale, 1.0f/16.0f, 128); 


			break;
		}
		
		default:
			mesh = new DemoMesh("cylinder_1", collision, NULL, NULL, NULL);
			break;
	}
*/

	entity->SetMesh(mesh);
	mesh->Release();
}

void DemoEntityManager::SerializedPhysicScene (const char* const name)
{
	FILE* const file = fopen (name, "wb");

	// we can save anything we want with the serialized data,
	// I am saving the camera orientation
	dMatrix camMatrix (m_cameraManager->GetCamera()->GetNextMatrix());
	SerializeFile (file, &camMatrix, sizeof (camMatrix));

	int bodyCount = NewtonWorldGetBodyCount(m_world);
	NewtonBody** array = new NewtonBody*[bodyCount];

	int index = 0;
	for (NewtonBody* body = NewtonWorldGetFirstBody(m_world); body; body = NewtonWorldGetNextBody(m_world, body)) {
		array[index] = body;
		index ++;
		_ASSERTE (index <= bodyCount);
	}
	NewtonSerializeBodyArray(m_world, array, bodyCount, BodySerialization, SerializeFile, file);

	delete[] array;
	fclose (file);
}

void DemoEntityManager::DeserializedPhysicScene (const char* const name)
{
	// add the sky
	CreateSkyBox();

	FILE* const file = fopen (name, "rb");

	if (file) {
		// read the application data use to initialize the engine and other application related stuff.
		// reading the camera orientation
		dMatrix camMatrix(GetIdentityMatrix());
	//	DeserializeFile (file, &camMatrix, sizeof (camMatrix));
	//	SetCameraMatrix(dQuaternion (camMatrix), camMatrix.m_posit);

		NewtonDeserializeBodyArray(m_world, BodyDeserialization, DeserializeFile, file);
		fclose (file);
	}
}


dFloat DemoEntityManager::CalculateInteplationParam () const
{
	unsigned64 timeStep = dGetTimeInMicrosenconds () - m_microsecunds;		
	dFloat param = (dFloat (timeStep) * MAX_PHYSICS_FPS) / 1.0e6f;
	_ASSERTE (param >= 0.0f);
	if (param > 1.0f) {
		param = 1.0f;
	}
	return param;
}

void DemoEntityManager::UpdateScene(float timestep)
{
//Sleep (20);

	// Make context current
	makeCurrent();

	unsigned64 time0 = dGetTimeInMicrosenconds ();

	// update the the state of all bodies in the scene
	UpdatePhysics(timestep);

	unsigned64 time1 = dGetTimeInMicrosenconds ();
	m_mainThreadPhysicsTime = dFloat ((time1 - time0) / 1000.0f);
	time0 = time1;

	// Get the interpolated location of each body in the scene
	m_cameraManager->InterpolateMatrices (this, CalculateInteplationParam());

	// Our shading model--Goraud (smooth). 
	glShadeModel (GL_SMOOTH);

	// Culling. 
	glCullFace (GL_BACK);
	glFrontFace (GL_CCW);
	glEnable (GL_CULL_FACE);

	//	glEnable(GL_DITHER);

	// z buffer test
	glEnable(GL_DEPTH_TEST);
	glDepthFunc (GL_LEQUAL);

	glHint(GL_PERSPECTIVE_CORRECTION_HINT, GL_FASTEST);
	glHint(GL_POLYGON_SMOOTH_HINT, GL_FASTEST);

	glClearColor (0.5f, 0.5f, 0.5f, 0.0f );
	//glClear( GL_COLOR_BUFFER_BIT );
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);


	// set default lightning
//	glDisable(GL_BLEND);
	glEnable (GL_LIGHTING);

	// make sure the model view matrix is set to identity before setting world space ligh sources
	glMatrixMode(GL_MODELVIEW);
	glLoadIdentity();

	dFloat cubeColor[] = { 1.0f, 1.0f, 1.0f, 1.0 };
	glMaterialfv(GL_FRONT, GL_SPECULAR, cubeColor);
	glMaterialfv(GL_FRONT, GL_AMBIENT_AND_DIFFUSE, cubeColor);
	glMaterialf(GL_FRONT, GL_SHININESS, 50.0);
	glTexEnvf(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_MODULATE);

	// one light form the Camera eye point
	GLfloat lightDiffuse0[] = { 0.5f, 0.5f, 0.5f, 0.0 };
	GLfloat lightAmbient0[] = { 0.0f, 0.0f, 0.0f, 0.0 };
	dVector camPosition (m_cameraManager->GetCamera()->m_matrix.m_posit);
	GLfloat lightPosition0[] = {camPosition.m_x, camPosition.m_y, camPosition.m_z};

	glLightfv(GL_LIGHT0, GL_POSITION, lightPosition0);
	glLightfv(GL_LIGHT0, GL_AMBIENT, lightAmbient0);
	glLightfv(GL_LIGHT0, GL_DIFFUSE, lightDiffuse0);
	glLightfv(GL_LIGHT0, GL_SPECULAR, lightDiffuse0);
	glEnable(GL_LIGHT0);


	// set just one directional light
	GLfloat lightDiffuse1[] = { 0.7f, 0.7f, 0.7f, 0.0 };
	GLfloat lightAmbient1[] = { 0.2f, 0.2f, 0.2f, 0.0 };
	GLfloat lightPosition1[] = { -500.0f, 200.0f, 500.0f, 0.0 };

	glLightfv(GL_LIGHT1, GL_POSITION, lightPosition1);
	glLightfv(GL_LIGHT1, GL_AMBIENT, lightAmbient1);
	glLightfv(GL_LIGHT1, GL_DIFFUSE, lightDiffuse1);
	glLightfv(GL_LIGHT1, GL_SPECULAR, lightDiffuse1);
	glEnable(GL_LIGHT1);

	// update Camera
	m_cameraManager->GetCamera()->SetViewMatrix(getWidth(), getHeight());


	// render all entities
	if (m_mainWindow->m_hideVisualMeshes) {
		if (m_sky) {
			glPushMatrix();	
			m_sky->Render(timestep);
			glPopMatrix();
		}

	} else {
		for (dListNode* node = GetFirst(); node; node = node->GetNext()) {
			DemoEntity* const entity = node->GetInfo();
			glPushMatrix();	
			entity->Render(timestep);
			glPopMatrix();
		}
	}

	m_cameraManager->RenderPickedTarget ();

	if (m_mainWindow->m_showContactPoints) {
		RenderContactPoints (GetNewton());
	}

	if (m_mainWindow->m_showNormalForces) {
		RenderNormalForces (GetNewton());
		// see if there is a vehicle controller and 
		void* const vehListerNode =  NewtonWorldGetPreListener (GetNewton(), VEHICLE_PLUGIN_NAME);
		if (vehListerNode) {
			CustomControllerBase* const manager = (CustomControllerBase*) NewtonWorldGetListenerUserData(GetNewton(), vehListerNode);
			manager->Debug();
		}

		void* const characterListerNode =  NewtonWorldGetPreListener (GetNewton(), PLAYER_PLUGIN_NAME);
		if (characterListerNode) {
			CustomControllerBase* const manager = (CustomControllerBase*) NewtonWorldGetListenerUserData(GetNewton(), characterListerNode);
			manager->Debug();
		}
	}

	if (m_mainWindow->m_showAABB) {
		RenderAABB (GetNewton());

	}


//	DEBUG_DRAW_MODE mode = m_solid;
	DEBUG_DRAW_MODE mode = m_lines;
	if (m_mainWindow->m_debugDisplayState) {
		//glDisable (GL_LIGHTING);
		glDisable(GL_TEXTURE_2D);
		DebugRenderWorldCollision (GetNewton(), mode);
		glEnd();
	}

	if (m_renderHood) {
		// set display for 2d render mode

		dFloat width = GetWidth();
		dFloat height = GetHeight();

		glColor3f(1.0, 1.0, 1.0);

		glMatrixMode(GL_PROJECTION);
		glPushMatrix();
		glLoadIdentity();
		//glOrtho(0.0f, width, 0.0f, height, 0.0, 1.0);
		gluOrtho2D(0, width, 0, height);

		glMatrixMode(GL_MODELVIEW);
		glPushMatrix();
		glLoadIdentity();

		glDisable(GL_LIGHTING);
		glDisable(GL_DEPTH_TEST);
		glEnable(GL_TEXTURE_2D);	

		glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);


		// render 2d display
		m_renderHood (this, m_renderHoodContext);

		// restore display mode
		glPopMatrix();

		glMatrixMode(GL_PROJECTION);
		glPopMatrix();

		glMatrixMode(GL_MODELVIEW);
	}


	// do all 2d drawing
	m_profiler.m_nextLine = 0;
	int profileFlags = 0;
	for (int i = 0; i < int (sizeof (m_showProfiler) / sizeof (m_showProfiler[0])); i ++) {
		profileFlags |=  m_showProfiler[i] ? (1 << i) : 0;
	}

	if (m_mainWindow->m_concurrentProfilerState) {
		m_profiler.RenderConcurrentPerformance();
	}

	if (profileFlags) {
		m_profiler.Render (profileFlags);
	}

	if (m_mainWindow->m_threadProfilerState) {
		m_profiler.RenderThreadPerformance ();
	}

	swapBuffers();

	// Make context non-current
	makeNonCurrent();


	time1 = dGetTimeInMicrosenconds ();
	m_mainThreadGraphicsTime = dFloat ((time1 - time0) / 1000.0f);
}



void DemoEntityManager::UpdatePhysics(float timestep)
{
	// read the controls 
	// update the physics
	if (m_world) {
//Sleep (10);

		dFloat timestepInSecunds = 1.0f / MAX_PHYSICS_FPS;
		unsigned64 timestepMicrosecunds = unsigned64 (timestepInSecunds * 1000000.0f);

		unsigned64 currentTime = dGetTimeInMicrosenconds ();
		unsigned64 nextTime = currentTime - m_microsecunds;
		int loops = 0;

		while ((nextTime >= timestepMicrosecunds) && (loops < MAX_PHYSICS_LOOPS)) {
			loops ++;

			// run the newton update function
			if (!m_reEntrantUpdate) {
				m_reEntrantUpdate = true;
				if (m_physicsUpdate && m_world) {

					ClearDebugDisplay(m_world);

					// update the physics world
					if (!m_mainWindow->m_physicsUpdateMode) {
						NewtonUpdate (m_world, timestepInSecunds);
					} else {
						NewtonUpdateAsync(m_world, timestepInSecunds);
					}
					m_physThreadTime = NewtonReadPerformanceTicks (m_world, NEWTON_PROFILER_WORLD_UPDATE) * 1.0e-3f;
				}
				m_reEntrantUpdate = false;
			}

			nextTime -= timestepMicrosecunds;
			m_microsecunds += timestepMicrosecunds;
		}

		if (loops) {
			m_physicsTime = dFloat (dGetTimeInMicrosenconds () - currentTime) / 1000000.0f;

			if (m_physicsTime >= MAX_PHYSICS_LOOPS * (1.0f / MAX_PHYSICS_FPS)) {
				m_microsecunds = currentTime;
			}
		}
	}
}


