import bpy
from mathutils import geometry

bl_info = {
    "name" : "Export Bezier control points as transforms.",
    "category" : "Object"
}

class BezierToTransformsOperator (bpy.types.Operator):
    """Export Bezier control points as transforms"""
    bl_idname = "object.bezier_to_transforms"
    bl_label = "Export Bezier control points as transforms"
    bl_options = {'REGISTER', 'UNDO'}

    @classmethod
    def poll(cls, context):
        return (context.object is not None and context.object.type == 'CURVE')

    def execute(self, context):
        path = context.object.data
        if len(path.splines) > 1:
            return {'CANCELLED'}

        bpy.ops.object.mode_set(mode='OBJECT')

        b_points = path.splines[0].bezier_points
        bpy.ops.object.empty_add(location = context.object.location)
        path_object = context.active_object
        path_object.name = "Path"

        index = 0
        last_index = len(b_points)-1
        for point in b_points:
            if index == 0:
                bpy.ops.object.empty_add(location = point.co)
                context.active_object.name = "C{0}".format(index)
                context.active_object.parent = path_object
                index += 1
                
                bpy.ops.object.empty_add(location = point.handle_right)
                context.active_object.name = "C{0}".format(index)
                context.active_object.parent = path_object
                index += 1
            else:
                bpy.ops.object.empty_add(location = point.handle_left)
                context.active_object.name = "C{0}".format(index)
                context.active_object.parent = path_object
                index += 1
                
                bpy.ops.object.empty_add(location = point.co)
                context.active_object.name = "C{0}".format(index)
                context.active_object.parent = path_object
                index += 1
                
                if point != b_points[last_index]:
                    bpy.ops.object.empty_add(location = point.handle_right)
                    context.active_object.name = "C{0}".format(index)
                    context.active_object.parent = path_object
                    index += 1
                elif path.splines[0].use_cyclic_u:
                    bpy.ops.object.empty_add(location = point.handle_right)
                    context.active_object.name = "C{0}".format(index)
                    context.active_object.parent = path_object
                    index += 1    
                    
                    bpy.ops.object.empty_add(location = b_points[0].handle_left)
                    context.active_object.name = "C{0}".format(index)
                    context.active_object.parent = path_object
                                              
        return {'FINISHED'}


class BezierToTransformsPanel (bpy.types.Panel):
    """Export Bezier control points as transforms"""
    bl_label = "Export Bezier control points as transforms"
    bl_idname = "OBJECT_PT_bezier_to_transforms"
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_context = "data"

    @classmethod
    def poll(cls, context):
        return (context.object is not None and context.object.type == 'CURVE')

    def draw(self, context):
        layout = self.layout
        row = layout.row()        
        operator = row.operator(BezierToTransformsOperator.bl_idname, text = "Create")
        
def register():
    bpy.utils.register_class(BezierToTransformsPanel)
    bpy.utils.register_class(BezierToTransformsOperator)

def unregister():
    bpy.utils.unregister_class(BezierToTransformsPanel)
    bpy.utils.unregister_class(BezierToTransformsOperator)


if __name__ == "__main__":
    register()
    


