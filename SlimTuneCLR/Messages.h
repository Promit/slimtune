/*
* Copyright (c) 2007-2009 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
#ifndef MESSAGES_H
#define MESSAGES_H
#pragma once

class IProfilerServer;

enum MessageId
{
	MID_MapFunction = 0x01,
	MID_MapClass,
	MID_MapModule,
	MID_MapAssembly,

	MID_EnterFunction = 0x10,
	MID_LeaveFunction,
	MID_TailCall,

	MID_ObjectAllocated = 0x20,
	MID_CollectionStarted,
	MID_CollectionFinished,

	MID_TransitionOut = 0x30,		//From managed to unmanaged (out of the runtime)
	MID_TransitionIn,				//From unmanaged to managed (back into the runtime)

	MID_CreateThread = 0x40,
	MID_DestroyThread,
	MID_NameThread,

	MID_Sample = 0x50,

	MID_BeginEvent = 0xf0,
	MID_EndEvent
};

enum ClientRequest
{
	CR_GetFunctionMapping = 0x01,
	CR_GetClassMapping,
	CR_GetModuleMapping,
	CR_GetAssemblyMapping,

	CR_GetThreadInfo = 0x10,
};

namespace Messages
{
	struct MapFunction
	{
		static const int MaxNameSize = 1024;
		static const int MaxSignatureSize = 2048;

		unsigned int FunctionId;
		unsigned int ClassId;
		unsigned int IsNative;
		wchar_t Name[MaxNameSize];
		wchar_t Signature[MaxSignatureSize];

		void Write(IProfilerServer& server, size_t nameCount, size_t signatureCount);
	};

	struct MapClass
	{
		static const int MaxNameSize = 1024;

		unsigned int ClassId;
		wchar_t Name[MaxNameSize];

		void Write(IProfilerServer& server, size_t nameCount);
	};

	//Used for enter, leave, tailcall
	struct FunctionELT
	{
		unsigned int ThreadId;
		unsigned int FunctionId;
		unsigned __int64 TimeStamp;

		void Write(IProfilerServer& server, MessageId id);
	};

	struct ObjectAllocated
	{
		ClassID ClassId;
		size_t Size;
	};

	//Also used for DestroyThread
	struct CreateThread
	{
		unsigned int ThreadId;

		void Write(IProfilerServer& server, MessageId id);
	};

	struct NameThread
	{
		static const int MaxNameSize = 256;

		unsigned int ThreadId;
		wchar_t Name[MaxNameSize];

		void Write(IProfilerServer& server, size_t nameCount);
	};

	struct Sample
	{
		unsigned int ThreadId;
		std::vector<unsigned int> Functions;

		void Write(IProfilerServer& server);
	};
}

namespace Requests
{
	struct GetFunctionMapping
	{
		unsigned int FunctionId;

		static GetFunctionMapping Read(char* buffer, size_t& bytesRead);
	};

	struct GetClassMapping
	{
		unsigned int ClassId;

		static GetClassMapping Read(char* buffer, size_t& bytesRead);
	};

	struct GetThreadInfo
	{
		unsigned int ThreadId;
	};
}

#endif
