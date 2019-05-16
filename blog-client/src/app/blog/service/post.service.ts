import { Injectable } from '@angular/core';
import { BaseService } from '../../shared/base.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { PostParameters } from '../model/Post-parameters';
import { Post } from '../model/post';
import { PostAdd } from '../model/post-add';

@Injectable({
  providedIn: 'root'
})
export class PostService extends BaseService {
  constructor(private http: HttpClient) {
    super();
  }

  getPagedPosts(postParameter?: any | PostParameters) {
    return this.http.get(`${this.apiUrlBase}/posts`, {
      headers: new HttpHeaders({
        Accept: 'application/vnd.zyz.hateoas+json'
      }),
      observe: 'response',
      params: postParameter
    });
  }

  addPost(post: PostAdd) {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/vnd.post.create+json',
        Accept: 'application/vnd.zyz.hateoas+json'
      })
    };

    return this.http.post<Post>(`${this.apiUrlBase}/posts`, post, httpOptions);
  }
}
