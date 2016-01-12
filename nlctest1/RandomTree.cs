using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nlctest1 {
    class RandomTree {
        public RandomTreeNode root;

        public RandomTree() {
            root = null;
        }

        public RandomTreeNode Find(int key) {
            return root?.Find(key);
        }

        public RandomTreeNode FindGreatestNotGreater(int key) {
            return root?.FindGreatestNotGreater(key);
        }

        public void Insert(int key, byte[] data) {
            root = RandomTreeNode.Insert(root, key, data);
        }

        public void Remove(int key) {
            root = RandomTreeNode.Remove(root, key);
        }
    }

    class RandomTreeNode {
        private static Random rnd = new Random();

        public int key;
        public int size;
        RandomTreeNode left;
        RandomTreeNode right;
        public byte[] data;

        RandomTreeNode(int k) {
            key = k;
            left = right = null;
            size = 1;
            data = new byte[32];
        }

        RandomTreeNode(int k, byte[] d) {
            key = k;
            left = right = null;
            size = 1;
            data = d;
        }

        public RandomTreeNode Find(int k) {
            if(k == key)
                return this;
            if(k < key)
                return left?.Find(k);
            else
                return right?.Find(k);
        }

        public RandomTreeNode FindGreatestNotGreater(int k, RandomTreeNode max = null) {
            if(key < k && (max == null || key>max.key)) {
                max = this;
            }
            if(k == key)
                return this;
            if(k < key)
                return left == null ? max : left.FindGreatestNotGreater(k, max);
            else
                return right == null ? max : right.FindGreatestNotGreater(k, max);
        }

        static public RandomTreeNode Insert(RandomTreeNode p, int k, byte[] d) {
            if(p == null) return new RandomTreeNode(k, d);
            if(rnd.Next(p.size + 1) == 0)
                return InsertRoot(p, k, d);
            if(p.key > k)
                p.left = Insert(p.left, k, d);
            else
                p.right = Insert(p.right, k, d);
            FixSize(p);
            return p;
        }

        static public RandomTreeNode Remove(RandomTreeNode p, int k) {
            if(p == null) return p;
            if(p.key == k) {
                RandomTreeNode q = join(p.left, p.right);
                p = null;
                return q;
            } else if(k < p.key)
                p.left = Remove(p.left, k);
            else
                p.right = Remove(p.right, k);
            return p;
        }



        static private RandomTreeNode InsertRoot(RandomTreeNode p, int k, byte[] d) {
            if(p == null) return new RandomTreeNode(k, d);
            if(k < p.key) {
                p.left = InsertRoot(p.left, k, d);
                return rotateright(p);
            } else {
                p.right = InsertRoot(p.right, k, d);
                return rotateleft(p);
            }
        }

        static private int GetSize(RandomTreeNode p) {
            if(p == null) return 0;
            return p.size;
        }
        static private void FixSize(RandomTreeNode p) {
            p.size = GetSize(p.left) + GetSize(p.right) + 1;
        }

        static private RandomTreeNode rotateright(RandomTreeNode p) {
            RandomTreeNode q = p.left;
            if(q == null) return p;
            p.left = q.right;
            q.right = p;
            q.size = p.size;
            FixSize(p);
            return q;
        }

        static private RandomTreeNode rotateleft(RandomTreeNode q) {
            RandomTreeNode p = q.right;
            if(p == null) return q;
            q.right = p.left;
            p.left = q;
            p.size = q.size;
            FixSize(q);
            return p;
        }

        static private RandomTreeNode join(RandomTreeNode p, RandomTreeNode q) {
            if(p == null) return q;
            if(q == null) return p;
            if(rnd.Next(p.size + q.size) < p.size) {
                p.right = join(p.right, q);
                FixSize(p);
                return p;
            } else {
                q.left = join(p, q.left);
                FixSize(q);
                return q;
            }
        }

    }
}
