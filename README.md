# AHD2-DeferredPipelineTest
## 优化方向考虑

可能的优化方向，记录在此，后续有空再深入研究。

* 把attachment和纹理分离。不直接将attachment作为纹理？根据[游戏引擎随笔 0x07：现代图形 API 的同步 - 知乎](https://zhuanlan.zhihu.com/p/100162469)。对于纹理和attachment，GPU有着不同的内存布局优化。（不过这是根据具体GPU来的，有的也没有这玩意）
* renderpass只在管线生命周期中指定一次，而不是现在的每帧add，clear所有pass。
* 深度图设置不合规，应该单独Blit一个的
