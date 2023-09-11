//
//  Helper.swift
//  Maui.Watch Watch App
//
//  Created by Jack Butler on 8/3/23.
//

import Foundation
import SystemPackage
import os

public func log(_ line: String) {
    do {
        let dtFormatter = DateFormatter()
        dtFormatter.dateFormat = "MM/dd/yyyy, hh:mm:ss a"
        let formattedDate = dtFormatter.string(from: Date.now)
        let newLine = "[\(formattedDate)]: \(line)\n"
        var newFile = false
        if !FileManager.default.fileExists(atPath: Helper.getLogPath()) {
            newFile = true
        }
        let fd = try FileDescriptor.open(Helper.logPath, .readWrite, options: [ .append, .create, .sharedLock], permissions: .ownerReadWrite)
        try fd.closeAfter {
            if newFile {
                if let version = Bundle.main.infoDictionary?["CFBundleVersion"] as? String {
                    _ = try fd.writeAll("[\(formattedDate)]: KinesiaU.OnWatch version \(version)\n".utf8)
                }
            }
            _ = try fd.writeAll(newLine.utf8)
        }
        /*
        guard let data = newLine.data(using: String.Encoding.utf8) else {return}
        if FileManager.default.fileExists(atPath: Helper.getLogPath()) {
            if let fileHandle = try? FileHandle(forWritingTo: Helper.logURL) {
                try fileHandle.seekToEnd()
                try fileHandle.write(contentsOf: data)
                try fileHandle.close()
            }
        } else {
            try data.write(to: Helper.logURL, options: .atomic)
        }
        */
        print("[\(formattedDate)]: \(line)")
    }
    catch {
        print("\(Date.now.formatted()): \(error.localizedDescription)")
    }
}

struct Helper {
    public static var logPath: FilePath {
        get {
            return FilePath(getLogPath())
        }
    }
    
    public static var logURL: URL {
        get {
            var file = "WatchLog.txt"
            let dtFormatter = DateFormatter()
            dtFormatter.dateFormat = "yyyy-MM-dd"
            let formattedDate = dtFormatter.string(from: Date.now)
            file = "\(formattedDate).\(file)"
            let fileURL = storagePath.appendingPathComponent(file)
            return fileURL
        }
    }
    
    public static func getLogFileName() -> String {
        return logURL.lastPathComponent
    }
    
    public static func getLogPath() -> String {
        return logURL.pathString()
    }
    
    public static var storagePath: URL {
        get {
            var tempDir = FileManager.default.temporaryDirectory
            if #available(watchOS 9.0, *) {
                tempDir.append(path: "assessment_data")
            } else {
                tempDir = tempDir.appendingPathComponent("assessment_data", isDirectory: true)
            }
            if !FileManager.default.fileExists(atPath: tempDir.pathString()) {
                do {
                    try FileManager.default.createDirectory(at: tempDir, withIntermediateDirectories: true)
                }
                catch {
                    log("Error creating directory: \(error.localizedDescription)")
                }
            }
            return tempDir
        }
    }
}
extension URL {
    func pathString() -> String {
        var filePath: String = ""
        if #available(watchOS 9.0, *) {
            filePath = self.path(percentEncoded: true)
        } else {
            filePath = self.path
        }
        return filePath
    }
}
