# NGUITools
这是一个基于NGUI的资源检测工具

导入说明：导入Assets下任一文件夹即可（ LogEditor的第22行需改成CheckDebug.cs的实际路径）

并非原创，只是在别人的基础上进行增删功能
**原链接：https://www.cnblogs.com/yougoo/p/10075822.html**

本工具是基于NGUI的引用检测工具。工具包括几个部分：
1. Atlas/Sprite的引用查找
2. UITexture引用查找
3. Component查找
4. 字库引用查找
 一、使用场景
（这四种检测方法大同小异，因此重点介绍图集检测，其他方法可依此类推）
当图集的体积超出预期时，我们常常需要清理其中的无用图片，或剔除引用较少的图片。然而，如果仅靠手动检查每个预制体的节点来确认引用情况，不仅繁琐低效，还容易遗漏。
本工具旨在解决这一问题，无需逐一检查预制体节点，只需输入图集名称和图片名称，即可快速查询该 Sprite 在全局范围内的引用情况，大幅提升优化效率。
二、Atlas/Sprite的引用查找

- 检索文件夹：在Project视图选中一个文件夹，会检索该文件夹内的所有预制体的图集
- 检索GameObject：在Hierarchy选中一个预制体，只会检索该对象
- 检索时忽略字母大小写：输入的图集和精灵名字片段在查找时会忽略大小写。
- 替换：勾选时可选择新图集和改图集中的sprite以替换所查找的sprite
- 
输入图集名和精灵名，点击查找，会在Console打印出引用了对应图集和精灵的路径信息。双击路径信息，可以定位到Project对应Prefab的位置。（ps:图集名也可以通过拖拽图集到NGUI Atlas框内自动获取）
注意：如果不输入图集名和精灵名，将输出所有引用了Sprite的节点。如果只输入图集名，将输出引用了该图集的所有Sprite节点。
三、字库引用查找

检索字库为空的Label：勾选后，可查看一些字库丢失的节点。
四、UITexture引用查找

检索未赋值Texture：勾选后，可查看一些引用UITexture但没有赋值，或者Texture丢失的情况。
五、Component引用查找
如果不输入关键字，将会输出所有Component的引用节点
